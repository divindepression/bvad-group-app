import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Company, UpdateCompanyRequest } from '../models/company.model';

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private baseUrl = `${environment.apiUrl}/companies`;
  private http = inject(HttpClient);

  // 🗄 Cache pour éviter changement d'URL constant
  private assetCache = new Map<string, string>();

  getAll(): Observable<Company[]> {
    return this.http.get<Company[]>(this.baseUrl);
  }

  getById(id: string): Observable<Company> {
    return this.http.get<Company>(`${this.baseUrl}/${id}`);
  }

  update(id: string, data: UpdateCompanyRequest): Observable<Company> {
    return this.http.put<Company>(`${this.baseUrl}/${id}`, data);
  }

  // ═══════════════════════════════════════
  // 🖼 UPLOAD
  // ═══════════════════════════════════════
  uploadLogo(id: string, file: File): Observable<any> {
    return this.uploadAsset(id, file, 'logo');
  }

  uploadStamp(id: string, file: File): Observable<any> {
    return this.uploadAsset(id, file, 'stamp');
  }

  uploadDirectorSignature(id: string, file: File): Observable<any> {
    return this.uploadAsset(id, file, 'director-signature');
  }

  private uploadAsset(id: string, file: File, path: string): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/${id}/${path}`, formData);
  }

  // ═══════════════════════════════════════
  // 📥 URLs
  // ═══════════════════════════════════════
logoUrl(id: string): string {
  if (!this.assetCache.has(`${id}-logo`)) {
    this.assetCache.set(`${id}-logo`, `${this.baseUrl}/${id}/logo?t=${Date.now()}`);
  }
  return this.assetCache.get(`${id}-logo`)!;
}

  stampUrl(id: string): string {
    return this.cachedUrl(`${id}-stamp`, `${this.baseUrl}/${id}/stamp`);
  }

  directorSignatureUrl(id: string): string {
    return this.cachedUrl(`${id}-signature`, `${this.baseUrl}/${id}/director-signature`);
  }

  private cachedUrl(key: string, base: string): string {
    if (!this.assetCache.has(key)) {
      this.assetCache.set(key, `${base}?t=${Date.now()}`);
    }
    return this.assetCache.get(key)!;
  }

  create(data: any): Observable<Company> {
  return this.http.post<Company>(this.baseUrl, data);
}

delete(id: string): Observable<void> {
  return this.http.delete<void>(`${this.baseUrl}/${id}`);
}

  refreshCache(id: string): void {
    this.assetCache.delete(`${id}-logo`);
    this.assetCache.delete(`${id}-stamp`);
    this.assetCache.delete(`${id}-signature`);
  }
}