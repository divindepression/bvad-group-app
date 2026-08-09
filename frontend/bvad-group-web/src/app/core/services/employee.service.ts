import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateEmployeeRequest,
  Employee,
  EmployeeStatus,
} from '../models/employee.model';

export interface EmployeeFilters {
  companyId?: string;
  search?: string;
  status?: EmployeeStatus;
  department?: string;
}

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private apiUrl = `${environment.apiUrl}/employees`;

  constructor(private http: HttpClient) {}

  getAll(filters: EmployeeFilters = {}): Observable<Employee[]> {
    let params = new HttpParams();
    if (filters.companyId) params = params.set('companyId', filters.companyId);
    if (filters.search) params = params.set('search', filters.search);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.department)
      params = params.set('department', filters.department);

    return this.http.get<Employee[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateEmployeeRequest): Observable<Employee> {
    return this.http.post<Employee>(this.apiUrl, data);
  }

  updateById(id: string, data: Partial<Employee>): Observable<Employee> {
    return this.http.put<Employee>(`${this.apiUrl}/${id}`, data);
  }

  update(id: string, data: CreateEmployeeRequest): Observable<Employee> {
    return this.http.put<Employee>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  downloadSheetPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/sheet-pdf`, {
      responseType: 'blob',
    });
  }
}
