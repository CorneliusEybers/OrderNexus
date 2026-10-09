import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import {
    CurrencyLookup, ItemLookup, Lookup, OrderLineRequest,
    OrderRequest, OrderResponse, ProblemDetails, StatusLookup
} from './models/order.models';
import { OrderApiService } from './services/order-api.service';

type ScreenMode = 'list' | 'new' | 'view' | 'edit';

interface EditableOrderLine {
    itemId: number;
    quantity: number;
}

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './app.html',
    styleUrl: './app.css'
})
export class App implements OnInit {
    private readonly api = inject(OrderApiService);

    public readonly title = 'OrderNexus';
    public screen: ScreenMode = 'list';
    public orders: OrderResponse[] = [];
    public customers: Lookup[] = [];
    public salesReps: Lookup[] = [];
    public items: ItemLookup[] = [];
    public statuses: StatusLookup[] = [];
    public currencies: CurrencyLookup[] = [];

    public filterCustomerId = 0;
    public filterSalesRepId = 0;
    public filterReference = '';
    public filterStatusId = 0;

    public selectedOrder: OrderResponse | null = null;
    public customerId = 0;
    public salesRepId = 0;
    public currencyId = 0;
    public externalReference = '';
    public notes = '';
    public editableLines: EditableOrderLine[] = [];
    public newItemId = 0;
    public newItemQuantity = 1;
    public changeToStatusId = 0;

    public loading = false;
    public saving = false;
    public errorMessage = '';
    public successMessage = '';
    public validationMessage = '';

    public ngOnInit(): void {
        this.loadInitialData();
    }

    public get filteredOrders(): OrderResponse[] {
        const reference = this.filterReference.trim().toLowerCase();
        return this.orders.filter(ord => {
            return (this.filterCustomerId === 0 || ord.customerId === this.filterCustomerId)
                && (this.filterSalesRepId === 0 || ord.salesRepId === this.filterSalesRepId)
                && (this.filterStatusId === 0 || ord.orderStatusId === this.filterStatusId)
                && (reference.length === 0 || ord.externalReference.toLowerCase().includes(reference));
        });
    }

    public get isEditable(): boolean {
        return this.screen === 'new' || this.screen === 'edit';
    }

    public get canModifySelected(): boolean {
        return this.selectedOrder?.statusCode.toUpperCase() === 'PENDING';
    }

    public get availableStatuses(): StatusLookup[] {
        if (this.selectedOrder === null) {
            return [];
        }
        const currentCode = this.selectedOrder.statusCode.toUpperCase();
        if (currentCode === 'PENDING') {
            return this.statuses.filter(stt => stt.code === 'CONFIRMED' || stt.code === 'CANCELLED');
        }
        if (currentCode === 'CONFIRMED') {
            return this.statuses.filter(stt => stt.code === 'FULFILLED' || stt.code === 'CANCELLED');
        }
        return [];
    }

    public get displayedLines(): { item: ItemLookup; quantity: number; lineTotal: number }[] {
        return this.editableLines.map(line => {
            const item = this.items.find(itm => itm.id === line.itemId);
            if (item === undefined) {
                return null;
            }
            // This is a UI estimate; the API computes authoritative totals.
            const previous = this.selectedOrder?.items.find(orditm => orditm.itemId === line.itemId);
            const price = previous?.unitPrice ?? item.unitPrice;
            return { item: { ...item, unitPrice: price }, quantity: line.quantity, lineTotal: price * line.quantity };
        }).filter((line): line is { item: ItemLookup; quantity: number; lineTotal: number } => line !== null);
    }

    public get estimatedTotal(): number {
        return this.displayedLines.reduce((sum, line) => sum + line.lineTotal, 0);
    }

    public get selectedNewItem(): ItemLookup | undefined {
        return this.items.find(itm => itm.id === Number(this.newItemId));
    }

    public loadInitialData(): void {
        this.loading = true;
        this.errorMessage = '';
        forkJoin({
            orders: this.api.getOrders(),
            customers: this.api.getCustomers(),
            salesReps: this.api.getSalesReps(),
            items: this.api.getItems(),
            statuses: this.api.getStatuses(),
            currencies: this.api.getCurrencies()
        }).subscribe({
            next: data => {
                this.orders = data.orders;
                this.customers = data.customers;
                this.salesReps = data.salesReps;
                this.items = data.items;
                this.statuses = data.statuses;
                this.currencies = data.currencies;
                this.currencyId = data.currencies[0]?.id ?? 0;
                this.loading = false;
            },
            error: error => {
                this.errorMessage = this.describeError(error);
                this.loading = false;
            }
        });
    }

    public refreshOrders(): void {
        this.loading = true;
        this.api.getOrders().subscribe({
            next: orders => {
                this.orders = orders;
                this.loading = false;
            },
            error: error => {
                this.errorMessage = this.describeError(error);
                this.loading = false;
            }
        });
    }

    public showNew(): void {
        this.clearMessages();
        this.selectedOrder = null;
        this.screen = 'new';
        this.customerId = this.customers[0]?.id ?? 0;
        this.salesRepId = this.salesReps[0]?.id ?? 0;
        this.currencyId = this.currencies[0]?.id ?? 0;
        this.externalReference = '';
        this.notes = '';
        this.editableLines = [];
        this.newItemId = this.items[0]?.id ?? 0;
        this.newItemQuantity = 1;
    }

    public showOrder(id: number, edit: boolean = false): void {
        this.clearMessages();
        this.loading = true;
        this.api.getOrder(id).subscribe({
            next: order => {
                this.selectedOrder = order;
                this.screen = edit && order.statusCode.toUpperCase() === 'PENDING' ? 'edit' : 'view';
                this.customerId = order.customerId;
                this.salesRepId = order.salesRepId ?? 0;
                this.currencyId = order.currencyId;
                this.externalReference = order.externalReference;
                this.notes = order.notes ?? '';
                this.editableLines = order.items.map(orditm => ({ itemId: orditm.itemId, quantity: orditm.quantity }));
                this.newItemId = this.items[0]?.id ?? 0;
                this.newItemQuantity = 1;
                this.changeToStatusId = 0;
                this.loading = false;
            },
            error: error => {
                this.errorMessage = this.describeError(error);
                this.loading = false;
            }
        });
    }

    public backToList(): void {
        this.screen = 'list';
        this.selectedOrder = null;
        this.validationMessage = '';
        this.refreshOrders();
    }

    public addLine(): void {
        this.validationMessage = '';
        const itemId = Number(this.newItemId);
        const quantity = Number(this.newItemQuantity);
        if (!this.items.some(itm => itm.id === itemId)) {
            this.validationMessage = 'Please select a product.';
            return;
        }
        if (!Number.isInteger(quantity) || quantity <= 0) {
            this.validationMessage = 'Quantity must be a positive whole number.';
            return;
        }
        const previous = this.editableLines.find(line => line.itemId === itemId);
        if (previous !== undefined) {
            previous.quantity += quantity;
        } else {
            this.editableLines.push({ itemId, quantity });
        }
        this.newItemQuantity = 1;
    }

    public updateLineQuantity(itemId: number, quantity: number): void {
        const line = this.editableLines.find(orditm => orditm.itemId === itemId);
        if (line !== undefined) {
            line.quantity = Number(quantity);
        }
    }

    public removeLine(itemId: number): void {
        this.editableLines = this.editableLines.filter(line => line.itemId !== itemId);
    }

    public saveOrder(): void {
        this.validationMessage = '';
        this.errorMessage = '';
        if (this.customerId <= 0 || this.currencyId <= 0 || !this.externalReference.trim()) {
            this.validationMessage = 'Customer, currency and external reference are required.';
            return;
        }
        if (this.externalReference.trim().length > 100) {
            this.validationMessage = 'External reference cannot exceed 100 characters.';
            return;
        }
        if (this.editableLines.length === 0 || this.editableLines.some(line => !Number.isInteger(line.quantity) || line.quantity <= 0)) {
            this.validationMessage = 'Add at least one product with a positive whole-number quantity.';
            return;
        }
        const request: OrderRequest = {
            customerId: Number(this.customerId),
            salesRepId: this.salesRepId > 0 ? Number(this.salesRepId) : null,
            currencyId: Number(this.currencyId),
            externalReference: this.externalReference.trim(),
            notes: this.notes.trim() || null,
            items: this.editableLines.map(line => ({ itemId: line.itemId, quantity: Number(line.quantity) }))
        };
        this.saving = true;
        const operation = this.screen === 'edit' && this.selectedOrder !== null
            ? this.api.updateOrder(this.selectedOrder.id, request)
            : this.api.createOrder(request);
        operation.subscribe({
            next: order => {
                this.saving = false;
                this.successMessage = `Order ${order.externalReference} saved successfully.`;
                this.showOrder(order.id);
                // Dynamic: showOrder clears messages; restore success after initiating lookup.
                this.successMessage = `Order ${order.externalReference} saved successfully.`;
                this.refreshOrders();
            },
            error: error => {
                this.saving = false;
                this.errorMessage = this.describeError(error);
            }
        });
    }

    public changeStatus(): void {
        if (this.selectedOrder === null || this.changeToStatusId <= 0) {
            return;
        }
        this.saving = true;
        this.clearMessages();
        this.api.changeStatus(this.selectedOrder.id, Number(this.changeToStatusId)).subscribe({
            next: order => {
                this.saving = false;
                this.selectedOrder = order;
                this.changeToStatusId = 0;
                this.successMessage = `Status updated to ${order.statusCode}.`;
                this.refreshOrders();
            },
            error: error => {
                this.saving = false;
                this.errorMessage = this.describeError(error);
            }
        });
    }

    public deleteOrder(ord: OrderResponse): void {
        if (ord.statusCode.toUpperCase() !== 'PENDING') {
            this.errorMessage = 'Only Pending orders may be deleted.';
            return;
        }
        // Destructive action requires explicit user confirmation.
        if (!confirm(`Delete order ${ord.externalReference}? This cannot be undone.`)) {
            return;
        }
        this.saving = true;
        this.clearMessages();
        this.api.deleteOrder(ord.id).subscribe({
            next: () => {
                this.saving = false;
                this.successMessage = `Order ${ord.externalReference} deleted.`;
                this.screen = 'list';
                this.selectedOrder = null;
                this.refreshOrders();
            },
            error: error => {
                this.saving = false;
                this.errorMessage = this.describeError(error);
            }
        });
    }

    public clearFilters(): void {
        this.filterCustomerId = 0;
        this.filterSalesRepId = 0;
        this.filterStatusId = 0;
        this.filterReference = '';
    }

    public money(value: number, currency: string = 'ZAR'): string {
        return new Intl.NumberFormat('en-ZA', { style: 'currency', currency }).format(value);
    }

    public trackOrder(index: number, ord: OrderResponse): number {
        return ord.id;
    }

    private clearMessages(): void {
        this.errorMessage = '';
        this.successMessage = '';
        this.validationMessage = '';
    }

    private describeError(error: unknown): string {
        if (error instanceof HttpErrorResponse) {
            const problem = error.error as ProblemDetails | string | null;
            if (typeof problem === 'string' && problem.trim()) {
                return problem;
            }
            if (problem !== null && typeof problem === 'object') {
                const validation = problem.errors
                    ? Object.values(problem.errors).flat().join(' ')
                    : '';
                return validation || problem.detail || problem.title || `Request failed (${error.status}).`;
            }
            return error.status === 0
                ? 'Cannot reach the API. Check that OrderNexus.API is running and the Angular proxy is configured.'
                : `Request failed (${error.status}).`;
        }
        return 'An unexpected error occurred.';
    }
}
