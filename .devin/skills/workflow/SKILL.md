---
name: workflow
description: 'Flujo de trabajo por fase del proyecto OpenToWork. 8 etapas: identidad, planificacion, diseno, implementacion, pruebas, seguridad, correcciones, cierre.'
---

# OpenToWork - Flujo de Trabajo por Fase

## Regla Principal

**Ninguna fase puede iniciar hasta que la fase anterior este 100% completada y firmada por PM, QA y SEC.**

Cada fase tiene un flujo secuencial de 8 etapas (0 a 7). No se puede saltar etapas.

---

## Etapa 0: Declaracion de Identidad (Obligatoria)

Antes de iniciar cualquier trabajo, la IA debe declarar quien es.

### Identidades registradas

| IA | Carpeta de documentacion |
|---|---|
| **Iluna** | `docs/iluna/` |
| **Dsiezar** | `docs/dsiezar/` |

### Reglas

1. Al iniciar, decir: **"Soy Iluna"** o **"Soy Dsiezar"**
2. Todo cambio se documenta en `docs/{ia}/fase-N.md` usando `PLANTILLA.md`
3. Sin declaracion de identidad, no se puede continuar el flujo
4. Si ambas IAs trabajan, cada una documenta por separado

---

## Etapas 1-7

1. **Planificacion (PM):** Define alcance, desglosa tareas, asigna agentes, criterios de aceptacion
2. **Diseno Tecnico (PM + FS):** Disena entidades, endpoints, componentes, migraciones. SEC revisa riesgos
3. **Implementacion (FS):** Crea codigo, migraciones, i18n. Build con 0 errores
4. **Pruebas Funcionales (QA):** Ejecuta casos de prueba, valida UI/UX, i18n, responsive
5. **Auditoria Seguridad (SEC):** Audita JWT, inputs, dependencias, CORS, encriptacion
6. **Correcciones (FS):** Corrige bugs de QA y hallazgos de SEC (cicla a Etapa 4)
7. **Cierre y Aprobacion (PM):** Verifica criterios, actualiza docs, marca fase completada

---

## Diagrama

```
INICIO FASE N
      |
      v
[0] Declaracion de Identidad (Soy Iluna / Soy Dsiezar)
      |
      v
[1] Planificacion (PM)
      |
      v
[2] Diseno Tecnico (PM + FS + SEC review)
      |
      v
[3] Implementacion (FS) --build OK--> [4] Pruebas (QA)
      |                                    |
      |                              bugs? v no bugs
      |                              [6] Correcciones   [5] Auditoria (SEC)
      |                                   |                  |
      |                              <--- re-valida <---+    |
      |                                                        v
      |                                                  [7] Cierre (PM)
      |                                                        |
      |                                                  FASE COMPLETADA
```

---

## Reglas Estrictas

1. **Declaracion de identidad obligatoria** antes de cualquier trabajo
2. **Documentacion por IA** en `docs/iluna/` o `docs/dsiezar/`
3. **No saltar fases:** Fase N+1 no inicia hasta Fase N en Etapa 7
4. **No saltar etapas** dentro de una fase
5. **Build obligatorio:** `dotnet build OpenToWork.slnx` con 0 errores antes de QA
6. **Cero bugs criticos** para aprobar
7. **Cero hallazgos criticos** de SEC para aprobar
8. **Ciclo de correccion** Etapa 4 -> 6 -> 4 hasta cero issues
9. **Documentacion obligatoria** en NEURAL_MAP.md y bitacora de la IA
10. **i18n obligatorio** en ambos idiomas (es/en)
11. **Migracion obligatoria** para cambios en entidades
12. **Aprobacion triple:** PM + QA + SEC para cerrar fase
13. **Cada fase es una rama:** `{ia}-{fase}`. Nunca commitear a `main`
14. **Minimizar dependencias** entre fases paralelas
