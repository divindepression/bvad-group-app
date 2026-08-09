import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Employee } from '../models/employee.model';

@Injectable({ providedIn: 'root' })
export class CommitteeService {
  private apiUrl = `${environment.apiUrl}/committee`;
  private http = inject(HttpClient);

  getCommittee(companyId: string): Observable<Employee[]> {
    return this.http.get<Employee[]>(`${this.apiUrl}/${companyId}`);
  }
}