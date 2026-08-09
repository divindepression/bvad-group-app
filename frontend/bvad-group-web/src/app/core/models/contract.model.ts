export type ContractStatus =
  | 'Draft' | 'Active' | 'Suspended' | 'Terminated' | 'Expired' | 'Renewed';

export type ContractTypeStr =
  | 'CDI' | 'CDD' | 'Internship' | 'Freelance' | 'Apprenticeship';

export interface Contract {
  id: string;
  employeeId: string;
  employeeName: string;
  employeePosition?: string;
  companyId: string;
  companyName: string;
  companyColor: string;
  companyLogo?: string;
  contractNumber: string;
  contractType: ContractTypeStr;
  status: ContractStatus;
  position: string;
  department?: string;
  startDate: string;
  endDate?: string;
  signedDate?: string;
  salary: number;
  currency: string;
  trialPeriodMonths?: number;
  weeklyHours?: number;
  documentUrl?: string;
  documentFileName?: string;
  documentSize?: number;
  specialClauses?: string;
  notes?: string;
  remainingDays?: number;
  isExpiringSoon: boolean;
  isExpired: boolean;
  createdAt: string;
}

// Enums numériques pour le backend
export const ContractStatusValue = {
  Draft: 0, Active: 1, Suspended: 2,
  Terminated: 3, Expired: 4, Renewed: 5
} as const;

export const ContractTypeValueMap = {
  CDI: 0, CDD: 1, Internship: 2, Freelance: 3, Apprenticeship: 4
} as const;

export interface CreateContractRequest {
  employeeId: string;
  contractType: number;
  position: string;
  department?: string;
  startDate: string;
  endDate?: string;
  signedDate?: string;
  salary: number;
  currency: string;
  trialPeriodMonths?: number;
  weeklyHours?: number;
  specialClauses?: string;
  notes?: string;
  status: number;
}

// Labels
export const ContractStatusLabels: Record<ContractStatus, string> = {
  Draft: 'Brouillon',
  Active: 'En cours',
  Suspended: 'Suspendu',
  Terminated: 'Rompu',
  Expired: 'Expiré',
  Renewed: 'Renouvelé'
};

export const ContractStatusColors: Record<ContractStatus, string> = {
  Draft: 'bg-slate-500/20 text-slate-300 border-slate-500/40',
  Active: 'bg-green-500/20 text-green-400 border-green-500/40',
  Suspended: 'bg-orange-500/20 text-orange-400 border-orange-500/40',
  Terminated: 'bg-red-500/20 text-red-400 border-red-500/40',
  Expired: 'bg-red-500/20 text-red-400 border-red-500/40',
  Renewed: 'bg-blue-500/20 text-blue-400 border-blue-500/40'
};

export const ContractTypeLabels: Record<ContractTypeStr, string> = {
  CDI: 'CDI',
  CDD: 'CDD',
  Internship: 'Stage',
  Freelance: 'Prestation',
  Apprenticeship: 'Apprentissage'
};