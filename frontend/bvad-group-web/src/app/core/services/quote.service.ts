import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateQuoteRequest, Invoice, Quote, QuoteStatus } from '../models/billing.model';

@Injectable({ providedIn: 'root' })
export class QuoteService {
  private apiUrl = `${environment.apiUrl}/quotes`;
  private http = inject(HttpClient);

  getAll(companyId?: string, clientId?: string, status?: QuoteStatus): Observable<Quote[]> {
    let params = new HttpParams();
    if (companyId) params = params.set('companyId', companyId);
    if (clientId) params = params.set('clientId', clientId);
    if (status) params = params.set('status', status);
    return this.http.get<Quote[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Quote> {
    return this.http.get<Quote>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateQuoteRequest): Observable<Quote> {
    return this.http.post<Quote>(this.apiUrl, data);
  }

  update(id: string, data: CreateQuoteRequest): Observable<Quote> {
    return this.http.put<Quote>(`${this.apiUrl}/${id}`, data);
  }

  markAsSent(id: string): Observable<Quote> {
    return this.http.post<Quote>(`${this.apiUrl}/${id}/send`, {});
  }

  markAsAccepted(id: string): Observable<Quote> {
    return this.http.post<Quote>(`${this.apiUrl}/${id}/accept`, {});
  }

  markAsRejected(id: string, reason?: string): Observable<Quote> {
    return this.http.post<Quote>(`${this.apiUrl}/${id}/reject`, { reason });
  }

  convertToInvoice(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.apiUrl}/${id}/convert-to-invoice`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  downloadPdfUrl(id: string): string {
    return `${this.apiUrl}/${id}/pdf`;
  }

  downloadPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/pdf`, { responseType: 'blob' });
  }
}