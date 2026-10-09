import { useState } from 'react';
import {
  IonButton, IonCard, IonCardContent, IonCardHeader, IonCardSubtitle,
  IonCardTitle, IonChip, IonContent, IonHeader, IonNote, IonPage,
  IonRefresher, IonRefresherContent, IonSearchbar, IonSpinner, IonTitle,
  IonToolbar, useIonAlert, useIonViewWillEnter,
} from '@ionic/react';
import PageHeader from '../../components/PageHeader';
import { api, ApiError } from '../../services/api';
import type { CandidateResult } from '../../types';
import { useAuth } from '../../auth/AuthContext';

/**
 * Solo para empresas verificadas (la API devuelve 403 al resto). Los resultados
 * vienen anonimizados ("Maria G."); el perfil completo solo se abre cuando el
 * staff entrega el candidato tras la solicitud.
 */
export default function CandidateSearch() {
  const { logout } = useAuth();
  const [query, setQuery] = useState('');
  const [items, setItems] = useState<CandidateResult[]>([]);
  const [requested, setRequested] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [denied, setDenied] = useState(false);
  const [presentAlert] = useIonAlert();

  const load = async () => {
    try {
      const res = await api.get<{ items: CandidateResult[] } | CandidateResult[]>(
        `/api/candidates/search?pageSize=30${query ? `&query=${encodeURIComponent(query)}` : ''}`);
      setItems(Array.isArray(res) ? res : res.items ?? []);
      setDenied(false);
    } catch (e) {
      if (e instanceof ApiError && e.status === 403) setDenied(true);
      else if (e instanceof ApiError && e.status === 401) void logout();
    } finally {
      setLoading(false);
    }
  };

  useIonViewWillEnter(() => { void load(); });

  const request = async (c: CandidateResult) => {
    try {
      await api.post(`/api/candidates/${c.id}/request`);
      setRequested(prev => new Set(prev).add(c.id));
      await presentAlert({
        header: 'Solicitud enviada',
        message: 'El equipo de Trato Directo revisará tu solicitud y te entregará el candidato si encaja.',
        buttons: ['OK'],
      });
    } catch (e) {
      const msg = e instanceof ApiError && e.status === 409 ? 'Ya solicitaste este candidato.' : 'No se pudo enviar la solicitud.';
      await presentAlert({ header: 'Solicitud', message: msg, buttons: ['OK'] });
    }
  };

  return (
    <IonPage>
      <PageHeader title="Buscar candidatos">
        <IonToolbar>
          <IonSearchbar placeholder="Puesto, skill o sector" debounce={400}
            value={query} onIonInput={e => { setQuery(e.detail.value ?? ''); void load(); }} />
        </IonToolbar>
      </PageHeader>
      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={async e => { await load(); e.detail.complete(); }}>
          <IonRefresherContent />
        </IonRefresher>
        {loading && <div style={{ textAlign: 'center', padding: 40 }}><IonSpinner /></div>}
        {denied && (
          <p style={{ textAlign: 'center', color: 'var(--ion-color-tertiary)', padding: 40 }}>
            La búsqueda de candidatos está disponible para empresas verificadas por Trato Directo.
          </p>
        )}
        {!loading && !denied && items.length === 0 &&
          <p style={{ textAlign: 'center', color: 'var(--ion-color-tertiary)', padding: 40 }}>Sin resultados.</p>}
        {items.map(c => (
          <IonCard key={c.id}>
            <IonCardHeader>
              <IonCardSubtitle>{c.city ?? ''}</IonCardSubtitle>
              <IonCardTitle>{c.maskedName}{c.verifiedByTd && ' ✔'}</IonCardTitle>
            </IonCardHeader>
            <IonCardContent>
              {c.title && <p style={{ color: 'var(--ion-color-secondary)', marginTop: 0 }}>{c.title}</p>}
              {c.score != null && <IonChip color="primary">Score {Math.round(c.score)}</IonChip>}
              <div style={{ marginTop: 10 }}>
                <IonButton size="small" disabled={requested.has(c.id)} onClick={() => void request(c)}>
                  {requested.has(c.id) ? 'Solicitud enviada' : 'Solicitar candidato'}
                </IonButton>
                <IonNote style={{ display: 'block', marginTop: 8, fontSize: 12 }}>
                  El perfil completo lo entrega Trato Directo tras revisar la solicitud.
                </IonNote>
              </div>
            </IonCardContent>
          </IonCard>
        ))}
      </IonContent>
    </IonPage>
  );
}
