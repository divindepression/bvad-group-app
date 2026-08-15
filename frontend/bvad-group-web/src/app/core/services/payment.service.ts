import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreatePaymentRequest, Payment } from '../models/billing.model';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/payments`;
  private http = inject(HttpClient);

  getByInvoice(invoiceId: string): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.apiUrl}/invoice/${invoiceId}`);
  }

  record(data: CreatePaymentRequest): Observable<Payment> {
    return this.http.post<Payment>(this.apiUrl, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  downloadReceipt(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/receipt`, { responseType: 'blob' });
  }
}