import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CurrencyLookup, ItemLookup, 
         Lookup, OrderRequest,
         OrderResponse, StatusLookup } from '../models/order.models';

@Injectable({ providedIn: 'root' })
export class OrderApiService {
    private readonly http = inject(HttpClient);
    private readonly ordersUrl = '/api/orders';
    private readonly lookupsUrl = '/api/lookups';

    public getOrders(): Observable<OrderResponse[]> {
        return this.http.get<OrderResponse[]>(this.ordersUrl);
    }

    public getOrder(id: number): Observable<OrderResponse> {
        return this.http.get<OrderResponse>(`${this.ordersUrl}/${id}`);
    }

    public createOrder(request: OrderRequest): Observable<OrderResponse> {
        return this.http.post<OrderResponse>(this.ordersUrl, request);
    }

    public updateOrder(id: number, request: OrderRequest): Observable<OrderResponse> {
        return this.http.put<OrderResponse>(`${this.ordersUrl}/${id}`, request);
    }

    public deleteOrder(id: number): Observable<void> {
        return this.http.delete<void>(`${this.ordersUrl}/${id}`);
    }

    public changeStatus(id: number, orderStatusId: number): Observable<OrderResponse> {
        return this.http.patch<OrderResponse>(`${this.ordersUrl}/${id}/status`, { orderStatusId });
    }

    public getCustomers(): Observable<Lookup[]> {
        return this.http.get<Lookup[]>(`${this.lookupsUrl}/customers`);
    }

    public getSalesReps(): Observable<Lookup[]> {
        return this.http.get<Lookup[]>(`${this.lookupsUrl}/sales-reps`);
    }

    public getItems(): Observable<ItemLookup[]> {
        return this.http.get<ItemLookup[]>(`${this.lookupsUrl}/items`);
    }

    public getStatuses(): Observable<StatusLookup[]> {
        return this.http.get<StatusLookup[]>(`${this.lookupsUrl}/statuses`);
    }

    public getCurrencies(): Observable<CurrencyLookup[]> {
        return this.http.get<CurrencyLookup[]>(`${this.lookupsUrl}/currencies`);
    }
}
