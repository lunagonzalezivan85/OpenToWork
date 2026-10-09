import { useState } from 'react';
import {
  IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonContent,
  IonPage, IonSpinner, useIonViewWillEnter,
} from '@ionic/react';
import PageHeader from '../components/PageHeader';
import { api } from '../services/api';
import { useAuth } from '../auth/AuthContext';
import type { Application, Vacancy } from '../types';

export default function Dashboard() {
  const { user } = useAuth();
  const isCompany = user?.primaryRole === 1;
  const [stats, setStats] = useState<{ label: string; value: string }[] | null>(null);

  useIonViewWillEnter(() => {
    void (async () => {
      try {
        if (isCompany) {
          const vacancies = await api.get<Vacancy[]>('/api/permanentvacancies/my-company');
          setStats([{ label: 'Vacantes', value: String(vacancies.length) }]);
        } else {
          const apps = await api.get<Application[]>('/api/applications/my');
          setStats([{ label: 'Postulaciones', value: String(apps.length) }]);
        }
      } catch {
        setStats([]);
      }
    })();
  });

  return (
    <IonPage>
      <PageHeader title="Panel" />
      <IonContent className="ion-padding">
        <IonCard>
          <IonCardHeader><IonCardTitle>Hola, {user?.email}</IonCardTitle></IonCardHeader>
          <IonCardContent>
            {stats === null && <IonSpinner />}
            {stats?.map(s => <p key={s.label}><b>{s.value}</b> {s.label}</p>)}
            {!user?.emailVerified && <p style={{ color: 'var(--ion-color-warning)' }}>Tu correo todavía no está verificado.</p>}
          </IonCardContent>
        </IonCard>
      </IonContent>
    </IonPage>
  );
}
