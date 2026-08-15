import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateInvoiceRequest, Invoice, InvoiceStatus } from '../models/billing.model';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private apiUrl = `${environment.apiUrl}/invoices`;
  private http = inject(HttpClient);

  getAll(companyId?: string, clientId?: string, status?: InvoiceStatus, overdue?: boolean): Observable<Invoice[]> {
    let params = new HttpParams();
    if (companyId) params = params.set('companyId', companyId);
    if (clientId) params = params.set('clientId', clientId);
    if (status) params = params.set('status', status);
    if (overdue !== undefined) params = params.set('overdue', overdue);
    return this.http.get<Invoice[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateInvoiceRequest): Observable<Invoice> {
    return this.http.post<Invoice>(this.apiUrl, data);
  }

  update(id: string, data: CreateInvoiceRequest): Observable<Invoice> {
    return this.http.put<Invoice>(`${this.apiUrl}/${id}`, data);
  }

  issue(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.apiUrl}/${id}/issue`, {});
  }

  cancel(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.apiUrl}/${id}/cancel`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  downloadPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/pdf`, { responseType: 'blob' });
  }
}