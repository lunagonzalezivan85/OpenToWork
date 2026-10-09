import { useState } from 'react';
import {
  IonBadge, IonContent, IonItem, IonLabel, IonList, IonNote, IonPage,
  IonRefresher, IonRefresherContent, IonSpinner, useIonViewWillEnter,
} from '@ionic/react';
import PageHeader from '../../components/PageHeader';
import { api, ApiError } from '../../services/api';
import { useAuth } from '../../auth/AuthContext';

interface Delivery {
  id: string;
  candidateName: string;
  candidateTitle?: string;
  vacancyTitle: string;
  status: number;
  deliveredAt: string;
}

const STATUS: Record<number, string> = {
  0: 'Entregado', 1: 'Visto', 2: 'Contactado', 3: 'En proceso', 4: 'Contratado', 5: 'Descartado',
};

/** Candidatos que el staff ha entregado a tu empresa (post-entrega: identidad visible). */
export default function Deliveries() {
  const { logout } = useAuth();
  const [items, setItems] = useState<Delivery[]>([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    try {
      setItems(await api.get<Delivery[]>('/api/deliveries/my'));
    } catch (e) {
      if (e instanceof ApiError && e.status === 401) void logout();
    } finally {
      setLoading(false);
    }
  };

  useIonViewWillEnter(() => { void load(); });

  return (
    <IonPage>
      <PageHeader title="Candidatos entregados" />
      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={async e => { await load(); e.detail.complete(); }}>
          <IonRefresherContent />
        </IonRefresher>
        {loading && <div style={{ textAlign: 'center', padding: 40 }}><IonSpinner /></div>}
        {!loading && items.length === 0 &&
          <p style={{ textAlign: 'center', color: 'var(--ion-color-tertiary)', padding: 40 }}>
            Cuando el equipo entregue candidatos a tus vacantes aparecerán aquí.
          </p>}
        <IonList inset>
          {items.map(d => (
            <IonItem key={d.id}>
              <IonLabel>
                <h2>{d.candidateName}</h2>
                <p>{d.candidateTitle ?? ''} · {d.vacancyTitle}</p>
                <IonNote>Entregado {new Date(d.deliveredAt).toLocaleDateString('es-ES')}</IonNote>
              </IonLabel>
              <IonBadge slot="end" color="primary">{STATUS[d.status] ?? `Estado ${d.status}`}</IonBadge>
            </IonItem>
          ))}
        </IonList>
      </IonContent>
    </IonPage>
  );
}
