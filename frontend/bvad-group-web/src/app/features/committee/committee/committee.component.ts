import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal, Injector } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { CommitteeService } from '../../../core/services/committee.service';
import {
  CommitteePositionIcons,
  CommitteePositionLabels,
  Employee
} from '../../../core/models/employee.model';

@Component({
  selector: 'app-committee',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './committee.component.html'
})
export class CommitteeComponent {
  auth = inject(AuthService);
  private committeeService = inject(CommitteeService);
  private injector = inject(Injector);

  members = signal<Employee[]>([]);
  loading = signal(false);

  constructor() {
    // 🔥 allowSignalWrites autorise l'écriture dans les signals depuis l'effect
    effect(() => {
      const company = this.auth.currentCompany();
      if (company) {
        console.log('🏛 Chargement comité pour', company.name);
        this.load();
      }
    }, { allowSignalWrites: true });
  }

  load(): void {
    const company = this.auth.currentCompany();
    if (!company) return;

    this.loading.set(true);
    this.committeeService.getCommittee(company.id).subscribe({
      next: (data) => {
        console.log(`✅ ${data.length} membres du comité reçus`);
        this.members.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('❌ Erreur comité', err);
        this.loading.set(false);
      }
    });
  }

  positionLabel(pos: string): string {
    return CommitteePositionLabels[pos as keyof typeof CommitteePositionLabels] || pos;
  }

  positionIcon(pos: string): string {
    return CommitteePositionIcons[pos as keyof typeof CommitteePositionIcons] || '👤';
  }

  initials(emp: Employee): string {
    return (emp.firstName.charAt(0) + emp.lastName.charAt(0)).toUpperCase();
  }

  ceo(): Employee | undefined {
    return this.members().find(m => m.committeePosition === 'CEO');
  }

  otherMembers(): Employee[] {
    return this.members().filter(m => m.committeePosition !== 'CEO');
  }
}