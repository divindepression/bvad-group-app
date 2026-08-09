import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { MyProfile } from '../models/me.model';

@Injectable({ providedIn: 'root' })
export class MeService {
  private apiUrl = `${environment.apiUrl}/me`;
  private http = inject(HttpClient);

  profile = signal<MyProfile | null>(null);

  load(): void {
    this.http.get<MyProfile>(this.apiUrl).subscribe({
      next: (data) => this.profile.set(data),
      error: (err) => console.error('❌ Impossible de charger le profil', err)
    });
  }
}