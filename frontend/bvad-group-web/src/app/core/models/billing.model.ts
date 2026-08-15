// ═══════════════════════════════════════
// 👤 CLIENT
// ═══════════════════════════════════════
export type ClientType = 'Individual' | 'Company';

export interface Client {
  id: string;
  clientCode?: string;
  type: ClientType;
  name: string;
  displayName: string;
  contactPerson?: string;
  position?: string;
  legalForm?: string;
  registrationNumber?: string;
  taxNumber?: string;
  capital?: number;
  email?: string;
  phone?: string;
  secondaryPhone?: string;
  website?: string;
  address?: string;
  city?: string;
  country?: string;
  postalCode?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateClientRequest {
  type: number;
  name: string;
  contactPerson?: string;
  position?: string;
  legalForm?: string;
  registrationNumber?: string;
  taxNumber?: string;
  capital?: number;
  email?: string;
  phone?: string;
  secondaryPhone?: string;
  website?: string;
  address?: string;
  city?: string;
  country?: string;
  postalCode?: string;
  notes?: string;
}

export const ClientTypeValue = {
  Individual: 0,
  Company: 1
} as const;

// ═══════════════════════════════════════
// 📋 LINE ITEM (Quote + Invoice)
// ═══════════════════════════════════════
export interface LineItem {
  id?: string;
  order: number;
  description: string;
  unit?: string;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
  lineTotal?: number;
}

// ═══════════════════════════════════════
// 📝 QUOTE
// ═══════════════════════════════════════
export type QuoteStatus = 'Draft' | 'Sent' | 'Accepted' | 'Rejected' | 'Expired' | 'Converted';

export interface Quote {
  id: string;
  quoteNumber: string;
  companyId: string;
  companyName: string;
  companyColor: string;
  companyLogo?: string;
  clientId: string;
  clientName: string;
  clientDisplayName: string;
  issueDate: string;
  validUntil: string;
  currency: string;
  vatRate: number;
  subject?: string;
  notes?: string;
  paymentTerms?: string;
  subtotalHT: number;
  vatAmount: number;
  totalTTC: number;
  discountPercent: number;
  discountAmount: number;
  status: QuoteStatus;
  isExpired: boolean;
  sentAt?: string;
  acceptedAt?: string;
  rejectedAt?: string;
  convertedToInvoiceId?: string;
  lineItems: LineItem[];
  createdAt: string;
}

export interface CreateQuoteRequest {
  companyId: string;
  clientId: string;
  issueDate: string;
  validUntil: string;
  currency: string;
  vatRate: number;
  subject?: string;
  notes?: string;
  paymentTerms?: string;
  discountPercent: number;
  lineItems: LineItem[];
}

// ═══════════════════════════════════════
// 🧾 INVOICE
// ═══════════════════════════════════════
export type InvoiceStatus = 'Draft' | 'Issued' | 'PartiallyPaid' | 'Paid' | 'Overdue' | 'Cancelled';

export interface Invoice {
  id: string;
  invoiceNumber: string;
  companyId: string;
  companyName: string;
  companyColor: string;
  companyLogo?: string;
  clientId: string;
  clientName: string;
  clientDisplayName: string;
  issueDate: string;
  dueDate: string;
  paidAt?: string;
  currency: string;
  vatRate: number;
  subject?: string;
  notes?: string;
  paymentTerms?: string;
  subtotalHT: number;
  vatAmount: number;
  totalTTC: number;
  discountPercent: number;
  discountAmount: number;
  amountPaid: number;
  amountDue: number;
  status: InvoiceStatus;
  isOverdue: boolean;
  daysOverdue: number;
  fromQuoteId?: string;
  lineItems: LineItem[];
  payments: Payment[];
  createdAt: string;
}

export interface CreateInvoiceRequest {
  companyId: string;
  clientId: string;
  issueDate: string;
  dueDate: string;
  currency: string;
  vatRate: number;
  subject?: string;
  notes?: string;
  paymentTerms?: string;
  discountPercent: number;
  lineItems: LineItem[];
}

// ═══════════════════════════════════════
// 💳 PAYMENT
// ═══════════════════════════════════════
export type PaymentMethodType = 'Cash' | 'BankTransfer' | 'MobileMoney' | 'Check' | 'Card' | 'Other';
export type MobileMoneyOperatorType = 'None' | 'MTN' | 'Airtel' | 'Other';

export interface Payment {
  id: string;
  paymentNumber?: string;
  invoiceId: string;
  amount: number;
  currency: string;
  paymentDate: string;
  method: PaymentMethodType;
  mobileMoneyOperator?: MobileMoneyOperatorType;
  reference?: string;
  notes?: string;
  recordedByName?: string;
  createdAt: string;
}

export interface CreatePaymentRequest {
  invoiceId: string;
  amount: number;
  currency: string;
  paymentDate: string;
  method: number;
  mobileMoneyOperator?: number;
  reference?: string;
  notes?: string;
}

export const PaymentMethodValue = {
  Cash: 0,
  BankTransfer: 1,
  MobileMoney: 2,
  Check: 3,
  Card: 4,
  Other: 5
} as const;

export const MobileMoneyOperatorValue = {
  None: 0,
  MTN: 1,
  Airtel: 2,
  Other: 99
} as const;

// ═══════════════════════════════════════
// Labels / Helpers
// ═══════════════════════════════════════
export const QuoteStatusLabels: Record<QuoteStatus, string> = {
  Draft: 'Brouillon',
  Sent: 'Envoyé',
  Accepted: 'Accepté',
  Rejected: 'Refusé',
  Expired: 'Expiré',
  Converted: 'Converti en facture'
};

export const QuoteStatusColors: Record<QuoteStatus, string> = {
  Draft: 'bg-slate-500/20 text-slate-300 border-slate-500/40',
  Sent: 'bg-blue-500/20 text-blue-400 border-blue-500/40',
  Accepted: 'bg-green-500/20 text-green-400 border-green-500/40',
  Rejected: 'bg-red-500/20 text-red-400 border-red-500/40',
  Expired: 'bg-orange-500/20 text-orange-400 border-orange-500/40',
  Converted: 'bg-purple-500/20 text-purple-400 border-purple-500/40'
};

export const InvoiceStatusLabels: Record<InvoiceStatus, string> = {
  Draft: 'Brouillon',
  Issued: 'Émise',
  PartiallyPaid: 'Partiellement payée',
  Paid: 'Payée',
  Overdue: 'En retard',
  Cancelled: 'Annulée'
};

export const InvoiceStatusColors: Record<InvoiceStatus, string> = {
  Draft: 'bg-slate-500/20 text-slate-300 border-slate-500/40',
  Issued: 'bg-blue-500/20 text-blue-400 border-blue-500/40',
  PartiallyPaid: 'bg-orange-500/20 text-orange-400 border-orange-500/40',
  Paid: 'bg-green-500/20 text-green-400 border-green-500/40',
  Overdue: 'bg-red-500/20 text-red-400 border-red-500/40',
  Cancelled: 'bg-slate-500/20 text-slate-500 border-slate-500/40'
};

export const PaymentMethodLabels: Record<PaymentMethodType, string> = {
  Cash: '💵 Espèces',
  BankTransfer: '🏦 Virement',
  MobileMoney: '📱 Mobile Money',
  Check: '📝 Chèque',
  Card: '💳 Carte',
  Other: '🔄 Autre'
};

// Format money helper
export function formatMoney(amount: number, currency: string = 'XAF'): string {
  return new Intl.NumberFormat('fr-FR').format(amount) + ' ' + currency;
}