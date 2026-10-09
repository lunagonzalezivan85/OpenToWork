// Contratos compartidos con OpenToWork.API (espejo ligero de OpenToWork.Shared/DTOs).

export interface User {
  id: string;
  email: string;
  /** 0 = Candidato, 1 = Empresa, 2 = Staff */
  primaryRole: number;
  emailVerified: boolean;
  wizardCompleted: boolean;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface Vacancy {
  id: string;
  referenceCode: string;
  companyName: string;
  companyIsVerified: boolean;
  title: string;
  description?: string;
  location?: string;
  salaryMin?: number;
  salaryMax?: number;
  contractType: number;
  workMode: number;
  category?: string;
  matchPercentage?: number;
  distanceKm?: number;
}

export interface VacancySearchResult {
  items: Vacancy[];
  total: number;
  page: number;
  pageSize: number;
}

export interface Application {
  id: string;
  vacancyId: string;
  vacancyTitle: string;
  companyName?: string;
  status: number;
  createdAt: string;
}

export interface CandidateResult {
  id: string;
  maskedName: string;
  title?: string;
  city?: string;
  score?: number;
  verifiedByTd: boolean;
}

export interface Conversation {
  id: string;
  participantName: string;
  participantAvatar?: string;
  lastMessage?: string;
  unreadCount: number;
}

export interface Message {
  id: string;
  conversationId: string;
  content: string;
  sentAt: string;
  isMine: boolean;
  isRead: boolean;
}
