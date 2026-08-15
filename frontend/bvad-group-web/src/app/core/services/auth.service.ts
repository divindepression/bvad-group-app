import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CompanyAccessDto,
  LoginRequest,
  LoginResponse,
  UserDto
} from '../models/auth.model';
import { TokenService } from './token.service';

const USER_KEY = 'bvad_user';
const COMPANIES_KEY = 'bvad_companies';
const CURRENT_COMPANY_KEY = 'bvad_current_company';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private tokenService = inject(TokenService);
  private http = inject(HttpClient);
  private router = inject(Router);

  // 🔥 Signals Angular (moderne, réactif)
  currentUser = signal<UserDto | null>(this.getStoredUser());
  companies = signal<CompanyAccessDto[]>(this.getStoredCompanies());
  currentCompany = signal<CompanyAccessDto | null>(this.getStoredCurrentCompany());

  // Computed : est-ce que je suis connecté ?
  isLoggedIn = computed(() => !!this.currentUser());

  // 🔔 Événement émis après login/logout (pour SignalR + Notifications)
  loginSuccess$ = new EventEmitterLike<void>();
  logoutSuccess$ = new EventEmitterLike<void>();

  // ==============================
  // 🔐 Login
  // ==============================
  login(data: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, data).pipe(
      tap((res) => {
        this.handleLoginSuccess(res);
        this.loginSuccess$.emit();  // 🔔 Signal pour SignalR
      })
    );
  }

  private handleLoginSuccess(res: LoginResponse): void {
    this.tokenService.setToken(res.token);
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
    this.logoutSuccess$.emit();  // 🔔 Signal pour SignalR

    this.tokenService.removeToken();
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
    return this.tokenService.getToken();
  }

  // ==============================
  // 🎭 Rôle effectif dans la filiale active
  // ==============================
  getCurrentRole(): string {
    const user = this.currentUser();
    const company = this.currentCompany();

    // SuperAdmin / Admin voient tout partout
    if (user?.role === 'SuperAdmin' || user?.role === 'Admin') {
      return user.role;
    }

    // Sinon → rôle dans la filiale active
    return company?.role || user?.role || 'User';
  }

  // ==============================
  // 🎭 Libellé lisible du rôle
  // ==============================
  getCurrentRoleLabel(): string {
    const role = this.getCurrentRole();

    const labels: Record<string, string> = {
      SuperAdmin: 'Super Administrateur',
      Admin: 'Administrateur',
      User: 'Utilisateur',
      Director: 'Directeur',
      Manager: 'Manager',
      HR: 'Ressources Humaines',
      Accountant: 'Comptable',
      Employee: 'Employé'
    };

    return labels[role] || role;
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

// ==============================
// 📢 Mini EventEmitter (sans Angular EventEmitter pour rester léger)
// ==============================
class EventEmitterLike<T> {
  private listeners: Array<(value: T) => void> = [];

  subscribe(fn: (value: T) => void): { unsubscribe: () => void } {
    this.listeners.push(fn);
    return {
      unsubscribe: () => {
        this.listeners = this.listeners.filter(l => l !== fn);
      }
    };
  }

  emit(value: T): void {
    this.listeners.forEach(fn => fn(value));
  }
}