# Respuesta a las 17 Preguntas Estrategicas de RH

> **Responde:** Darwin (via Dsiezar)
> **Fecha:** 2026-08-15
> **En respuesta a:** `docs/rh/analisis-portafolio-candidatos.md`, seccion 4.
>
> Nota de nombre: el producto se llama **Trato Directo** (marca/negocio). "OpenToWork" es el nombre de trabajo del repositorio de codigo.

---

## 4.1 Modelo de Negocio

### 1. ¿El portafolio de candidatos es gratuito para candidatos y pago para empresas?

**Si.** Se mantiene el modelo ya definido en `docs/BUSINESS_PROPOSAL.md`: candidatos siempre gratis (son la fuente de datos y el efecto de red), el ingreso viene del lado de la empresa que contrata.

### 2. ¿Que modulos son del plan gratuito y cuales del plan premium?

Escalones para empresas:

| Plan | Incluye |
|---|---|
| **Free/Trial** | 1 vacante activa, ver candidatos sin score completo (solo datos basicos) |
| **Basic** | N vacantes, ver los 4 indices de evaluacion, busqueda simple |
| **Pro** | Filtros avanzados, ranking, exportacion, shortlist |
| **Enterprise** | Verificacion manual de referencias, soporte dedicado |

**Importante:** la plataforma es **standalone**, no se integra a ningun HRIS de la empresa (ni siquiera en Enterprise). Esto se confirma tambien en la pregunta 14.

### 3. ¿Se cobra por candidato contratado o por suscripcion mensual?

**Suscripcion mensual** como base (ingreso predecible, tipo SaaS), mas un **fee opcional por contratacion exitosa** solo para el servicio premium de verificacion manual de referencias. El core del negocio no se mezcla con "pay per hire".

---

## 4.2 Datos y Privacidad

### 4. ¿Quien es el dueno de los datos del candidato: la plataforma o la empresa?

El **candidato es dueno de sus datos personales**. La empresa es dueña de su propia actividad sobre esos datos (notas internas, estado del pipeline), no de los datos del candidato. La plataforma actua como custodio/procesador.

**Modelo de negocio derivado:** a la empresa se le ofrecen **candidatos ya verificados**. Todo candidato verificado debe llevar una **estrella o distintivo visual de "Verificado"** visible en su perfil y en las listas de candidatos — es la señal de confianza central del producto.

### 5. ¿El candidato puede eliminar su perfil y todos sus datos?

**Si**, el candidato puede eliminar su perfil cuando lo desee (derecho al olvido). Soft delete inmediato (ya es la convencion del proyecto) + purga definitiva tras un periodo de retencion corto, salvo obligacion legal de conservar.

### 6. ¿Las notas internas de reclutadores son accesibles al candidato?

**No**, por defecto privadas — para que el reclutador pueda ser honesto internamente. Se separa "notas internas" (privadas) de un "feedback estructurado" opcional que la empresa decide compartir con el candidato.

### 7. ¿Cuanto tiempo se conservan los datos de un candidato no contratado?

12 meses activos y descubrible por empresas. Despues de eso, **el candidato debe seguir existiendo en el panel de Admin** (para auditoria/historial), pero **deja de aparecer en las listas/busquedas que ve la empresa**. Es decir: retencion administrativa permanente, visibilidad comercial temporal.

---

## 4.3 Evaluacion y Scoring

### 8. ¿El scoring es transparente para el candidato?

**Transparencia parcial.** El candidato ve su score general y por categoria (ej. "Estabilidad: 70/100"), pero no el algoritmo/pesos exactos — para evitar que se "gamee" el sistema. Mismo balance que usa Uber con el rating de conductores.

### 9. ¿El candidato puede apelar un score bajo?

**No.** Una vez evaluado, si el candidato no cumple, no hay mecanismo de apelacion. La evaluacion es definitiva.

### 10. ¿Las empresas confian en el score de la plataforma o hacen su propia evaluacion?

El score es el **filtro central**, no solo un dato mas: **Trato Directo filtra a todas las personas** y determina el candidato ideal para cada empresa. La base de datos de candidatos que ve una empresa debe estar compuesta **unicamente por personal ya evaluado** (candidatos sin evaluar no se muestran a empresas — conecta con la pregunta 7).

Requisito derivado: debe existir un **proceso formal de evaluacion** de candidatos, con **auditoria y log de cuando se evaluo** cada uno (se apoya en `AD_AuditLog`, ya implementado en el Portal Admin).

Las empresas pueden ajustar el peso de cada indice por vacante (scorecard configurable, sugerido por RH en 2.1).

### 11. ¿Con que frecuencia se recalcula el score?

Recalculo **inmediato** cuando el candidato edita su perfil (evento), mas un recalculo **periodico mensual** para reflejar decaimiento por antiguedad (certificaciones vencidas, referencias viejas).

---

## 4.4 Competencia y Diferenciacion

### 12. ¿Que nos diferencia de LinkedIn, Computrabajo, Bumeran?

Ellos muestran informacion autodeclarada sin verificar. **Trato Directo verifica y scorea.** No se compite en volumen de vacantes, se compite en confiabilidad del match — es el objetivo central de `docs/BUSINESS_PROPOSAL.md`.

### 13. ¿El video pitch es nuestro diferenciador principal o es el scoring?

El **scoring/verificacion es el nucleo** (la propuesta de valor real). El video pitch es un complemento atractivo pero secundario — no se prioriza sobre la Fase 3 (Motor de Scoring).

### 14. ¿Las empresas pueden usar la plataforma sin integrar su HRIS?

**Si, siempre.** La plataforma funciona **100% standalone**. No se integra a ningun HRIS de ninguna empresa, en ningun plan (confirma y refuerza la pregunta 2).

---

## 4.5 Escalabilidad

### 15. ¿Cuantos candidatos y empresas soporta la plataforma en el lanzamiento?

Meta modesta y realista: soft launch regional, del orden de 500-1000 candidatos y 20-50 empresas piloto en un mercado acotado, antes de escalar. No sobre-diseñar para escala masiva desde el dia 1.

### 16. ¿El matching con IA escala linealmente o necesita infraestructura dedicada?

Para el volumen inicial, **reglas + scoring ponderado con SQL indexado alcanza y sobra** — no hace falta ML/infraestructura dedicada todavia. Se reevalua cuando el volumen se acerque a cientos de miles de perfiles.

### 17. ¿Se soportan multiples idiomas ademas de ES/EN?

Por ahora se mantiene **ES/EN** (cubre el mercado hispanohablante + internacional basico). Portugues seria el siguiente candidato natural si se expande a Brasil, pero no es prioridad hasta validar el modelo actual.

---

## Resumen de requisitos nuevos que estas respuestas generan para Fase 3/4

1. **Badge/estrella de "Verificado"** visible en perfil de candidato y en listas — falta implementar.
2. **Gate de visibilidad:** candidatos no evaluados, o cuya evaluacion vencio (retencion > 12 meses), no deben aparecer en las busquedas/listas de empresas, pero si deben seguir existiendo en el panel Admin.
3. **Proceso formal de evaluacion con auditoria**: cada evaluacion de candidato debe quedar registrada (quien/que motor la ejecuto, cuando) — se apoya en `AD_AuditLog` ya existente.
4. **Sin mecanismo de apelacion de score** — simplifica el alcance de Fase 3 (no hay que construir flujo de disputa/revision).
5. **Standalone confirmado**: ninguna fase debe incluir integracion con HRIS de terceros, ni como feature Enterprise.
6. **Scorecard configurable por vacante** (peso de cada indice ajustable por la empresa) — ya sugerido por RH, ahora confirmado como requisito.
