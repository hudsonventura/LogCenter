const normalizeBaseUrl = (value: string) => value.replace(/\/+$/, "");

export const getApiBaseUrl = () => {
  const configuredBaseUrl = import.meta.env.VITE_API_HOST;

  if (configuredBaseUrl) {
    return normalizeBaseUrl(configuredBaseUrl);
  }

  if (typeof window !== "undefined") {
    const { protocol, hostname, port, origin } = window.location;

    if (port === "9200") {
      return normalizeBaseUrl(origin);
    }

    return `${protocol}//${hostname}:9200`;
  }

  return "http://localhost:9200";
};
