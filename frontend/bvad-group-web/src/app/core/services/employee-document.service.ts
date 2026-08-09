import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateDocumentMetadata, EmployeeDocument } from '../models/employee-document.model';

@Injectable({ providedIn: 'root' })
export class EmployeeDocumentService {
  private baseUrl = `${environment.apiUrl}/employees`;
  private http = inject(HttpClient);

  // 🗄 Cache pour éviter les changements d'URL à chaque tick
  private photoUrlCache = new Map<string, string>();

  getByEmployee(employeeId: string): Observable<EmployeeDocument[]> {
    return this.http.get<EmployeeDocument[]>(`${this.baseUrl}/${employeeId}/documents`);
  }

  upload(employeeId: string, file: File, metadata: CreateDocumentMetadata): Observable<EmployeeDocument> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('metadata', JSON.stringify(metadata));
    return this.http.post<EmployeeDocument>(`${this.baseUrl}/${employeeId}/documents`, formData);
  }

  downloadUrl(employeeId: string, documentId: string): string {
    return `${this.baseUrl}/${employeeId}/documents/${documentId}/download`;
  }

  delete(employeeId: string, documentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${employeeId}/documents/${documentId}`);
  }

  // ═══════════════════════════════════════
  // 📸 Photo identité
  // ═══════════════════════════════════════
  uploadIdentityPhoto(employeeId: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/${employeeId}/identity-photo`, formData);
  }

  identityPhotoUrl(employeeId: string): string {
    // 🔥 Cache pour éviter le bug NG0100 (URL qui change à chaque tick)
    if (!this.photoUrlCache.has(employeeId)) {
      this.photoUrlCache.set(
        employeeId,
        `${this.baseUrl}/${employeeId}/identity-photo?t=${Date.now()}`
      );
    }
    return this.photoUrlCache.get(employeeId)!;
  }

  refreshPhotoCache(employeeId: string): void {
    this.photoUrlCache.delete(employeeId);
  }

  // ═══════════════════════════════════════
  // 🎫 Badge PDF
  // ═══════════════════════════════════════
  downloadBadge(employeeId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${employeeId}/badge`, { responseType: 'blob' });
  }
}