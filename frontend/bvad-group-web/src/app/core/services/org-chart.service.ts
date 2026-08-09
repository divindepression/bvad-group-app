import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OrgNode } from '../models/org-chart.model';

@Injectable({ providedIn: 'root' })
export class OrgChartService {
  private apiUrl = `${environment.apiUrl}/orgchart`;
  private http = inject(HttpClient);

  getOrgChart(companyId: string): Observable<OrgNode[]> {
    return this.http.get<OrgNode[]>(`${this.apiUrl}/${companyId}`);
  }
}