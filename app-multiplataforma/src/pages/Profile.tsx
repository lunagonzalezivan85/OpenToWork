import {
  IonButton, IonContent, IonHeader, IonItem, IonLabel, IonList, IonPage,
  IonTitle, IonToolbar, useIonAlert,
} from '@ionic/react';
import PageHeader from '../components/PageHeader';
import { useAuth } from '../auth/AuthContext';
import { api } from '../services/api';

export default function Profile() {
  const { user, logout } = useAuth();
  const [presentAlert] = useIonAlert();

  const revokeAll = async () => {
    try {
      const res = await api.post<{ revoked: number }>('/api/auth/revoke-all');
      await presentAlert({ header: 'Sesiones cerradas', message: `Se cerraron ${res.revoked} sesiones, incluida esta.`, buttons: ['OK'] });
      await logout();
    } catch {
      await presentAlert({ header: 'Error', message: 'No se pudieron cerrar las sesiones.', buttons: ['OK'] });
    }
  };

  return (
    <IonPage>
      <PageHeader title="Perfil" />
      <IonContent>
        <IonList inset>
          <IonItem><IonLabel><h2>{user?.email}</h2><p>{user?.primaryRole === 1 ? 'Cuenta de empresa' : 'Cuenta de candidato'}</p></IonLabel></IonItem>
          <IonItem><IonLabel>Correo verificado</IonLabel><IonLabel slot="end">{user?.emailVerified ? 'Sí' : 'No'}</IonLabel></IonItem>
        </IonList>
        <div style={{ padding: 20 }}>
          <IonButton expand="block" fill="outline" color="warning" onClick={() => void revokeAll()}>
            Cerrar sesión en todos los dispositivos
          </IonButton>
          <IonButton expand="block" color="danger" onClick={() => void logout()}>
            Cerrar sesión
          </IonButton>
        </div>
      </IonContent>
    </IonPage>
  );
}
