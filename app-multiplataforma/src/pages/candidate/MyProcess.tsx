import { useState } from 'react';
import {
  IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonContent,
  IonItem, IonLabel, IonList, IonNote, IonPage, IonSpinner,
  useIonViewWillEnter,
} from '@ionic/react';
import PageHeader from '../../components/PageHeader';
import { api, ApiError } from '../../services/api';
import { useAuth } from '../../auth/AuthContext';

interface ProcessDelivery { vacancyTitle: string; companyName: string; status: number; deliveredAt: string }
interface Process {
  hasRecruitment: boolean;
  currentStage: number;
  vacancyTitle?: string;
  vacancyCompanyName?: string;
  deliveries: ProcessDelivery[];
}

const STAGES = ['Sin proceso', 'En selección', 'Verificado', 'Listo para entrega', 'Entregado', 'En proceso', 'Incorporado'];

export default function MyProcess() {
  const { logout } = useAuth();
  const [p, setP] = useState<Process | null>(null);

  useIonViewWillEnter(() => {
    void (async () => {
      try { setP(await api.get<Process>('/api/candidates/me/process')); }
      catch (e) { if (e instanceof ApiError && e.status === 401) void logout(); }
    })();
  });

  return (
    <IonPage>
      <PageHeader title="Mi proceso" />
      <IonContent className="ion-padding">
        {!p && <div style={{ textAlign: 'center', padding: 40 }}><IonSpinner /></div>}
        {p && !p.hasRecruitment &&
          <p style={{ textAlign: 'center', color: 'var(--ion-color-tertiary)', padding: 40 }}>Aún no estás en ningún proceso de selección.</p>}
        {p?.hasRecruitment && (
          <IonCard>
            <IonCardHeader><IonCardTitle>{p.vacancyTitle ?? 'Proceso activo'}</IonCardTitle></IonCardHeader>
            <IonCardContent>
              <p>{p.vacancyCompanyName}</p>
              <p><b>Etapa:</b> {STAGES[p.currentStage] ?? `Etapa ${p.currentStage}`}</p>
            </IonCardContent>
          </IonCard>
        )}
        {p && p.deliveries.length > 0 && (
          <IonList inset>
            {p.deliveries.map((d, i) => (
              <IonItem key={i}>
                <IonLabel><h3>{d.vacancyTitle}</h3><p>{d.companyName}</p></IonLabel>
                <IonNote slot="end">{new Date(d.deliveredAt).toLocaleDateString('es-ES')}</IonNote>
              </IonItem>
            ))}
          </IonList>
        )}
      </IonContent>
    </IonPage>
  );
}
