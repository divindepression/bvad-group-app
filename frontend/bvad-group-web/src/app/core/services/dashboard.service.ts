import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AbsentToday,
  Birthday,
  DashboardOverview,
  EmployeesByCompany,
  EmployeesByContract,
  EmployeesByDepartment,
  ExpiringContract,
  HiringTrend,
  LeavesByMonth
} from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/dashboard`;
  private http = inject(HttpClient);

  private params(companyId?: string, extra?: any): HttpParams {
    let p = new HttpParams();
    if (companyId) p = p.set('companyId', companyId);
    if (extra) {
      for (const k of Object.keys(extra)) {
        p = p.set(k, extra[k]);
      }
    }
    return p;
  }

  getOverview(companyId?: string): Observable<DashboardOverview> {
    return this.http.get<DashboardOverview>(`${this.apiUrl}/overview`, { params: this.params(companyId) });
  }

  getEmployeesByCompany(): Observable<EmployeesByCompany[]> {
    return this.http.get<EmployeesByCompany[]>(`${this.apiUrl}/employees-by-company`);
  }

  getEmployeesByDepartment(companyId?: string): Observable<EmployeesByDepartment[]> {
    return this.http.get<EmployeesByDepartment[]>(
      `${this.apiUrl}/employees-by-department`,
      { params: this.params(companyId) }
    );
  }

  getEmployeesByContract(companyId?: string): Observable<EmployeesByContract[]> {
    return this.http.get<EmployeesByContract[]>(
      `${this.apiUrl}/employees-by-contract`,
      { params: this.params(companyId) }
    );
  }

  getHiringTrend(companyId?: string, months = 12): Observable<HiringTrend[]> {
    return this.http.get<HiringTrend[]>(
      `${this.apiUrl}/hiring-trend`,
      { params: this.params(companyId, { months }) }
    );
  }

  getLeavesByMonth(companyId?: string, months = 12): Observable<LeavesByMonth[]> {
    return this.http.get<LeavesByMonth[]>(
      `${this.apiUrl}/leaves-by-month`,
      { params: this.params(companyId, { months }) }
    );
  }

  getExpiringContracts(companyId?: string, days = 60): Observable<ExpiringContract[]> {
    return this.http.get<ExpiringContract[]>(
      `${this.apiUrl}/expiring-contracts`,
      { params: this.params(companyId, { days }) }
    );
  }

  getUpcomingBirthdays(companyId?: string, days = 30): Observable<Birthday[]> {
    return this.http.get<Birthday[]>(
      `${this.apiUrl}/upcoming-birthdays`,
      { params: this.params(companyId, { days }) }
    );
  }

  getAbsentToday(companyId?: string): Observable<AbsentToday[]> {
    return this.http.get<AbsentToday[]>(
      `${this.apiUrl}/absent-today`,
      { params: this.params(companyId) }
    );
  }
}