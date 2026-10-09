export interface Lookup {
    id: number;
    name: string;
}

export interface ItemLookup extends Lookup {
    sku: string;
    unitPrice: number;
}

export interface StatusLookup extends Lookup {
    code: string;
}

export interface CurrencyLookup extends Lookup {
    isoCode: string;
}

export interface OrderLineRequest {
    itemId: number;
    quantity: number;
}

export interface OrderRequest {
    customerId: number;
    salesRepId: number | null;
    currencyId: number;
    externalReference: string;
    notes: string | null;
    items: OrderLineRequest[];
}

export interface OrderLineResponse {
    id: number;
    itemId: number;
    sku: string;
    name: string;
    quantity: number;
    unitPrice: number;
    lineTotal: number;
}

export interface OrderResponse {
    id: number;
    customerId: number;
    customerName: string;
    salesRepId: number | null;
    salesRepName: string | null;
    currencyId: number;
    currencyCode: string;
    orderStatusId: number;
    statusCode: string;
    externalReference: string;
    notes: string | null;
    createdDateTime: string;
    updatedDateTime: string | null;
    subtotal: number;
    total: number;
    items: OrderLineResponse[];
}

export interface ProblemDetails {
    title?: string;
    detail?: string;
    status?: number;
    errors?: Record<string, string[]>;
}
