import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { MeService } from '../../../core/services/me.service';
import {
  CommitteePositionIcons,
  CommitteePositionLabels,
  UserRoleLabels
} from '../../../core/models/employee.model';

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-profile.component.html'
})
export class MyProfileComponent implements OnInit {
  meService = inject(MeService);

  ngOnInit(): void {
    this.meService.load();
  }

  positionLabel(pos: string): string {
    return CommitteePositionLabels[pos as keyof typeof CommitteePositionLabels] || pos;
  }

  positionIcon(pos: string): string {
    return CommitteePositionIcons[pos as keyof typeof CommitteePositionIcons] || '👤';
  }

  roleLabel(role: string): string {
    return UserRoleLabels[role as keyof typeof UserRoleLabels] || role;
  }

  formatSalary(salary?: number): string {
    if (!salary) return '—';
    return new Intl.NumberFormat('fr-FR').format(salary) + ' FCFA';
  }

  initials(firstName: string, lastName: string): string {
    return (firstName.charAt(0) + lastName.charAt(0)).toUpperCase();
  }
}