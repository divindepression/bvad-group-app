import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CalendarLeave,
  CreateLeaveRequest,
  LeaveBalance,
  LeaveRequest,
  LeaveStatus,
  LeaveType
} from '../models/leave.model';

export interface LeaveFilters {
  companyId?: string;
  employeeId?: string;
  status?: LeaveStatus;
  fromDate?: string;
  toDate?: string;
}

@Injectable({ providedIn: 'root' })
export class LeaveService {
  private apiUrl = `${environment.apiUrl}/leaves`;
  private http = inject(HttpClient);

  // ═══ Types ═══
  getTypes(): Observable<LeaveType[]> {
    return this.http.get<LeaveType[]>(`${this.apiUrl}/types`);
  }

  // ═══ Soldes ═══
  getBalances(employeeId: string, year?: number): Observable<LeaveBalance[]> {
    let params = new HttpParams();
    if (year) params = params.set('year', year);
    return this.http.get<LeaveBalance[]>(`${this.apiUrl}/balances/${employeeId}`, { params });
  }

  // ═══ Demandes ═══
  getRequests(filters: LeaveFilters = {}): Observable<LeaveRequest[]> {
    let params = new HttpParams();
    if (filters.companyId) params = params.set('companyId', filters.companyId);
    if (filters.employeeId) params = params.set('employeeId', filters.employeeId);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.fromDate) params = params.set('fromDate', filters.fromDate);
    if (filters.toDate) params = params.set('toDate', filters.toDate);
    return this.http.get<LeaveRequest[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<LeaveRequest> {
    return this.http.get<LeaveRequest>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateLeaveRequest): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(this.apiUrl, data);
  }

  approve(id: string, comment?: string): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(`${this.apiUrl}/${id}/approve`, { comment });
  }

  reject(id: string, comment: string): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(`${this.apiUrl}/${id}/reject`, { comment });
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/cancel`, {});
  }

  // ═══ Calendrier ═══
  getCalendar(companyId: string, from?: string, to?: string): Observable<CalendarLeave[]> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<CalendarLeave[]>(`${this.apiUrl}/calendar/${companyId}`, { params });
  }
}