import { useState } from 'react';
import {
  IonBadge, IonContent, IonHeader, IonItem, IonLabel, IonList, IonNote,
  IonPage, IonRefresher, IonRefresherContent, IonSpinner, IonTitle, IonToolbar,
  useIonViewWillEnter,
} from '@ionic/react';
import { api, ApiError } from '../../services/api';
import type { Application } from '../../types';
import { useAuth } from '../../auth/AuthContext';

const STATUS: Record<number, { label: string; color: string }> = {
  0: { label: 'Enviada', color: 'primary' },
  1: { label: 'En revisión', color: 'warning' },
  2: { label: 'Entrevista', color: 'tertiary' },
  3: { label: 'Aceptada', color: 'success' },
  4: { label: 'Descartada', color: 'medium' },
};

export default function Applications() {
  const { logout } = useAuth();
  const [apps, setApps] = useState<Application[]>([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    try {
      setApps(await api.get<Application[]>('/api/applications/my'));
    } catch (e) {
      if (e instanceof ApiError && e.status === 401) void logout();
    } finally {
      setLoading(false);
    }
  };

  useIonViewWillEnter(() => { void load(); });

  return (
    <IonPage>
      <IonHeader><IonToolbar><IonTitle>Mis postulaciones</IonTitle></IonToolbar></IonHeader>
      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={async e => { await load(); e.detail.complete(); }}>
          <IonRefresherContent />
        </IonRefresher>
        {loading && <div style={{ textAlign: 'center', padding: 40 }}><IonSpinner /></div>}
        {!loading && apps.length === 0 &&
          <p style={{ textAlign: 'center', color: 'var(--ion-color-tertiary)', padding: 40 }}>Todavía no has aplicado a ninguna vacante.</p>}
        <IonList inset>
          {apps.map(a => {
            const s = STATUS[a.status] ?? { label: `Estado ${a.status}`, color: 'medium' };
            return (
              <IonItem key={a.id}>
                <IonLabel>
                  <h2>{a.vacancyTitle}</h2>
                  <p>{a.companyName ?? ''}</p>
                  <IonNote>{new Date(a.createdAt).toLocaleDateString('es-ES')}</IonNote>
                </IonLabel>
                <IonBadge slot="end" color={s.color}>{s.label}</IonBadge>
              </IonItem>
            );
          })}
        </IonList>
      </IonContent>
    </IonPage>
  );
}
