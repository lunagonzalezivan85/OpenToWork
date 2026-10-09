import { useState } from 'react';
import {
  IonButton, IonContent, IonInput, IonItem, IonList, IonPage,
  IonText, IonToast, useIonRouter,
} from '@ionic/react';
import { useAuth } from '../auth/AuthContext';

export default function Login() {
  const { login } = useAuth();
  const router = useIonRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submit = async () => {
    setBusy(true);
    setError(null);
    try {
      const user = await login(email.trim(), password);
      router.push(user.primaryRole === 1 ? '/company/vacancies' : '/candidate/vacancies', 'root');
    } catch {
      setError('Credenciales incorrectas o cuenta no activada.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <IonPage>
      <IonContent className="ion-padding">
        <div style={{ maxWidth: 420, margin: '15vh auto 0' }}>
          <h1 style={{ fontWeight: 800 }}>Trato Directo</h1>
          <p style={{ color: 'var(--ion-color-tertiary)' }}>Inicia sesión en tu cuenta</p>
          <IonList inset>
            <IonItem>
              <IonInput label="Email" labelPlacement="floating" type="email" autocomplete="email"
                value={email} onIonInput={e => setEmail(e.detail.value ?? '')} />
            </IonItem>
            <IonItem>
              <IonInput label="Contraseña" labelPlacement="floating" type="password" autocomplete="current-password"
                value={password} onIonInput={e => setPassword(e.detail.value ?? '')}
                onKeyDown={e => e.key === 'Enter' && submit()} />
            </IonItem>
          </IonList>
          <IonButton expand="block" onClick={submit} disabled={busy || !email || !password}>
            {busy ? 'Entrando…' : 'Iniciar sesión'}
          </IonButton>
          <IonText color="danger"><p style={{ textAlign: 'center' }}>{error}</p></IonText>
        </div>
        <IonToast isOpen={!!error} message={error ?? ''} duration={3000} onDidDismiss={() => setError(null)} />
      </IonContent>
    </IonPage>
  );
}
