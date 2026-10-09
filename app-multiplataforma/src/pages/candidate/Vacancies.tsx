import { useEffect, useState } from 'react';
import {
  IonBadge, IonButton, IonCard, IonCardContent, IonCardHeader, IonCardSubtitle,
  IonCardTitle, IonChip, IonContent, IonHeader, IonItem, IonLabel, IonList,
  IonNote, IonPage, IonRefresher, IonRefresherContent, IonSearchbar, IonSpinner,
  IonTitle, IonToolbar, useIonAlert, useIonLoading, useIonViewWillEnter,
} from '@ionic/react';
import PageHeader from '../../components/PageHeader';
import { api, ApiError } from '../../services/api';
import type { Application, Vacancy, VacancySearchResult } from '../../types';
import { useAuth } from '../../auth/AuthContext';

const STATUS: Record<number, { label: string; color: string }> = {
  0: { label: 'Enviada', color: 'primary' },
  1: { label: 'En revisión', color: 'warning' },
  2: { label: 'Entrevista', color: 'tertiary' },
  3: { label: 'Aceptada', color: 'success' },
  4: { label: 'Descartada', color: 'medium' },
};

export default function Vacancies() {
  const { user, logout } = useAuth();
  const [query, setQuery] = useState('');
  const [vacancies, setVacancies] = useState<Vacancy[]>([]);
  const [applied, setApplied] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [presentAlert] = useIonAlert();
  const [presentLoading, dismissLoading] = useIonLoading();

  const load = async () => {
    try {
      const [search, myApps] = await Promise.all([
        api.get<VacancySearchResult>(`/api/vacancies/search?pageSize=30${query ? `&query=${encodeURIComponent(query)}` : ''}`),
        api.get<Application[]>('/api/applications/my').catch(() => [] as Application[]),
      ]);
      setVacancies(search.items);
      setApplied(new Set(myApps.map(a => a.vacancyId)));
    } catch (e) {
      if (e instanceof ApiError && e.status === 401) void logout();
    } finally {
      setLoading(false);
    }
  };

  useIonViewWillEnter(() => { void load(); });
  useEffect(() => { void load(); }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const apply = async (v: Vacancy) => {
    await presentLoading({ message: 'Enviando candidatura…' });
    try {
      await api.post('/api/applications', { vacancyId: v.id });
      setApplied(prev => new Set(prev).add(v.id));
      await presentAlert({ header: 'Candidatura enviada', message: `${v.title} — ${v.companyName}`, buttons: ['OK'] });
    } catch (e) {
      await presentAlert({ header: 'No se pudo aplicar', message: e instanceof ApiError ? e.message : 'Error', buttons: ['OK'] });
    } finally {
      await dismissLoading();
    }
  };

  return (
    <IonPage>
      <PageHeader title="Vacantes">
        <IonToolbar>
          <IonSearchbar placeholder="Puesto, empresa o palabra clave" debounce={400}
            value={query} onIonInput={e => { setQuery(e.detail.value ?? ''); void load(); }} />
        </IonToolbar>
      </PageHeader>
      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={async e => { await load(); e.detail.complete(); }}>
          <IonRefresherContent />
        </IonRefresher>
        {loading && <div style={{ textAlign: 'center', padding: 40 }}><IonSpinner /></div>}
        {!loading && vacancies.length === 0 &&
          <p style={{ textAlign: 'center', color: 'var(--ion-color-tertiary)', padding: 40 }}>Sin vacantes para esa búsqueda.</p>}
        {vacancies.map(v => (
          <IonCard key={v.id}>
            <IonCardHeader>
              <IonCardSubtitle>{v.companyName}{v.companyIsVerified && ' ✔'} {v.location && `· ${v.location}`}</IonCardSubtitle>
              <IonCardTitle>{v.title}</IonCardTitle>
            </IonCardHeader>
            <IonCardContent>
              <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', marginBottom: 10 }}>
                {v.category && <IonChip className="td-chip">{v.category}</IonChip>}
                {v.matchPercentage != null && <IonChip color="primary">{v.matchPercentage}% match</IonChip>}
                {(v.salaryMin || v.salaryMax) &&
                  <IonChip>{v.salaryMin ?? ''}{v.salaryMin && v.salaryMax ? '–' : ''}{v.salaryMax ?? ''} €</IonChip>}
              </div>
              {v.description && <p style={{ color: 'var(--ion-color-secondary)', margin: '0 0 12px' }}>{v.description.slice(0, 160)}{v.description.length > 160 && '…'}</p>}
              <IonButton size="small" disabled={applied.has(v.id)} onClick={() => void apply(v)}>
                {applied.has(v.id) ? 'Ya aplicaste' : 'Postularme'}
              </IonButton>
            </IonCardContent>
          </IonCard>
        ))}
      </IonContent>
    </IonPage>
  );
}
