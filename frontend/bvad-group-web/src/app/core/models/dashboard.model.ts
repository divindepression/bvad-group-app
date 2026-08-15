export interface DashboardOverview {
  totalEmployees: number;
  activeEmployees: number;
  totalCompanies: number;
  totalContracts: number;
  activeContracts: number;
  expiringContracts: number;
  pendingLeaveRequests: number;
  employeesOnLeaveToday: number;
  totalMonthlySalary: number;
  upcomingBirthdays: number;
  newEmployeesThisMonth: number;
}

export interface EmployeesByCompany {
  companyId: string;
  companyName: string;
  companyColor: string;
  companyLogo?: string;
  count: number;
  activeCount: number;
  onLeaveCount: number;
  totalSalary: number;
}

export interface EmployeesByDepartment {
  department: string;
  count: number;
}

export interface EmployeesByContract {
  contractType: string;
  count: number;
}

export interface HiringTrend {
  year: number;
  month: number;
  monthLabel: string;
  count: number;
}

export interface LeavesByMonth {
  year: number;
  month: number;
  monthLabel: string;
  totalRequests: number;
  totalDays: number;
  approved: number;
  pending: number;
  rejected: number;
}

export interface ExpiringContract {
  id: string;
  contractNumber: string;
  employeeId: string;
  employeeName: string;
  companyName: string;
  companyColor: string;
  position: string;
  endDate: string;
  daysRemaining: number;
}

export interface Birthday {
  employeeId: string;
  employeeName: string;
  photoUrl?: string;
  companyName: string;
  companyColor: string;
  position: string;
  birthDate: string;
  ageThisYear: number;
  daysUntil: number;
}

export interface AbsentToday {
  employeeId: string;
  employeeName: string;
  photoUrl?: string;
  companyName: string;
  companyColor: string;
  leaveTypeName: string;
  leaveTypeIcon: string;
  startDate: string;
  endDate: string;
  daysLeft: number;
}