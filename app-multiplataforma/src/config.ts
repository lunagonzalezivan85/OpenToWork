/// URL base de la API. En dev el backend corre en localhost:5100; en produccion
/// conviene servir la API en el mismo dominio o configurar VITE_API_URL.
export const API_URL: string = import.meta.env.VITE_API_URL ?? 'http://localhost:5100';
