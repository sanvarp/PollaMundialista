// Production defaults. The API base URL is injected at deploy time (M7).
// For SWA + Container Apps these are separate origins, so a full URL is required.
export const environment = {
  production: true,
  apiBaseUrl: 'https://REPLACE_WITH_CONTAINER_APP_URL/api',
};
