import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Contract,
  ContractStatus,
  CreateContractRequest
} from '../models/contract.model';

export interface ContractFilters {
  companyId?: string;
  employeeId?: string;
  status?: ContractStatus;
  type?: string;
  expiringSoon?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ContractService {
  private apiUrl = `${environment.apiUrl}/contracts`;
  private http = inject(HttpClient);

  getAll(filters: ContractFilters = {}): Observable<Contract[]> {
    let params = new HttpParams();
    if (filters.companyId) params = params.set('companyId', filters.companyId);
    if (filters.employeeId) params = params.set('employeeId', filters.employeeId);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.type) params = params.set('type', filters.type);
    if (filters.expiringSoon !== undefined) params = params.set('expiringSoon', filters.expiringSoon);
    return this.http.get<Contract[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Contract> {
    return this.http.get<Contract>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateContractRequest): Observable<Contract> {
    return this.http.post<Contract>(this.apiUrl, data);
  }

  update(id: string, data: CreateContractRequest): Observable<Contract> {
    return this.http.put<Contract>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // 📄 Télécharger le PDF généré automatiquement
  downloadPdfUrl(id: string): string {
    return `${this.apiUrl}/${id}/pdf`;
  }

  downloadPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/pdf`, { responseType: 'blob' });
  }

  // 📤 Upload contrat signé
  uploadSigned(id: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/${id}/upload`, formData);
  }

  // 📥 Télécharger le doc signé
  downloadSignedUrl(id: string): string {
    return `${this.apiUrl}/${id}/signed-document`;
  }
}