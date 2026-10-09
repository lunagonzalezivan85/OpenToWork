import { useState } from 'react';
import {
  IonBadge, IonContent, IonHeader, IonItem, IonLabel, IonList, IonNote,
  IonPage, IonRefresher, IonRefresherContent, IonSpinner, IonTitle, IonToolbar,
  useIonViewWillEnter,
} from '@ionic/react';
import PageHeader from '../../components/PageHeader';
import { api, ApiError } from '../../services/api';
import type { Vacancy } from '../../types';
import { useAuth } from '../../auth/AuthContext';

interface MyVacancy extends Vacancy {
  status?: number;
}

export default function MyVacancies() {
  const { logout } = useAuth();
  const [items, setItems] = useState<MyVacancy[]>([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    try {
      setItems(await api.get<MyVacancy[]>('/api/permanentvacancies/my-company'));
    } catch (e) {
      if (e instanceof ApiError && e.status === 401) void logout();
    } finally {
      setLoading(false);
    }
  };

  useIonViewWillEnter(() => { void load(); });

  return (
    <IonPage>
      <PageHeader title="Mis vacantes" />
      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={async e => { await load(); e.detail.complete(); }}>
          <IonRefresherContent />
        </IonRefresher>
        {loading && <div style={{ textAlign: 'center', padding: 40 }}><IonSpinner /></div>}
        {!loading && items.length === 0 &&
          <p style={{ textAlign: 'center', color: 'var(--ion-color-tertiary)', padding: 40 }}>Sin vacantes publicadas.</p>}
        <IonList inset>
          {items.map(v => (
            <IonItem key={v.id}>
              <IonLabel>
                <h2>{v.title}</h2>
                <p>{v.location ?? ''} {v.referenceCode && `· ${v.referenceCode}`}</p>
              </IonLabel>
              <IonNote slot="end">{v.category ?? ''}</IonNote>
            </IonItem>
          ))}
        </IonList>
      </IonContent>
    </IonPage>
  );
}
