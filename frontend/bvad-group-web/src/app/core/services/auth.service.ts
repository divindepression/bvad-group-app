import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CompanyAccessDto,
  LoginRequest,
  LoginResponse,
  UserDto
} from '../models/auth.model';

const TOKEN_KEY = 'bvad_token';
const USER_KEY = 'bvad_user';
const COMPANIES_KEY = 'bvad_companies';
const CURRENT_COMPANY_KEY = 'bvad_current_company';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  // 🔥 Signals Angular (moderne, réactif)
  currentUser = signal<UserDto | null>(this.getStoredUser());
  companies = signal<CompanyAccessDto[]>(this.getStoredCompanies());
  currentCompany = signal<CompanyAccessDto | null>(this.getStoredCurrentCompany());

  // Computed : est-ce que je suis connecté ?
  isLoggedIn = computed(() => !!this.currentUser());

  constructor(private http: HttpClient, private router: Router) {}

  // ==============================
  // 🔐 Login
  // ==============================
  login(data: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, data).pipe(
      tap((res) => this.handleLoginSuccess(res))
    );
  }

  private handleLoginSuccess(res: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(USER_KEY, JSON.stringify(res.user));
    localStorage.setItem(COMPANIES_KEY, JSON.stringify(res.companies));

    const defaultCompany = res.companies.find(c => c.isDefault) || res.companies[0];
    localStorage.setItem(CURRENT_COMPANY_KEY, JSON.stringify(defaultCompany));

    this.currentUser.set(res.user);
    this.companies.set(res.companies);
    this.currentCompany.set(defaultCompany);
  }

  // ==============================
  // 🚪 Logout
  // ==============================
  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(COMPANIES_KEY);
    localStorage.removeItem(CURRENT_COMPANY_KEY);

    this.currentUser.set(null);
    this.companies.set([]);
    this.currentCompany.set(null);

    this.router.navigate(['/login']);
  }

  // ==============================
  // 🏢 Changer de filiale active
  // ==============================
  switchCompany(company: CompanyAccessDto): void {
    localStorage.setItem(CURRENT_COMPANY_KEY, JSON.stringify(company));
    this.currentCompany.set(company);
  }

  // ==============================
  // 🎫 Token
  // ==============================
  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  // ==============================
  // 💾 Storage helpers
  // ==============================
  private getStoredUser(): UserDto | null {
    const stored = localStorage.getItem(USER_KEY);
    return stored ? JSON.parse(stored) : null;
  }

  private getStoredCompanies(): CompanyAccessDto[] {
    const stored = localStorage.getItem(COMPANIES_KEY);
    return stored ? JSON.parse(stored) : [];
  }

  private getStoredCurrentCompany(): CompanyAccessDto | null {
    const stored = localStorage.getItem(CURRENT_COMPANY_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}