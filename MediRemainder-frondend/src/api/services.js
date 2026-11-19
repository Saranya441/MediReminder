import { apiClient } from "./client";

const data = (response) => response.data?.data;

export const authApi = {
  login: async (payload) => data(await apiClient.post("/auth/login", payload)),
  register: async (payload) => data(await apiClient.post("/auth/register", payload)),
  google: async (credential) => data(await apiClient.post("/auth/google", { credential })),
  me: async () => data(await apiClient.get("/auth/me")),
};

export const dashboardApi = { get: async () => data(await apiClient.get("/dashboard")) };
export const medicinesApi = {
  list: async (params = {}) => data(await apiClient.get("/medicines", { params })),
  create: async (items) => data(await apiClient.post("/medicines", items)),
  update: async (id, payload) => data(await apiClient.put(`/medicines/${id}`, payload)),
  remove: async (id) => data(await apiClient.delete(`/medicines/${id}`)),
};
export const scheduleApi = {
  forDate: async (date) => data(await apiClient.get("/schedule/date", { params: { date } })),
  today: async () => data(await apiClient.get("/schedule/today")),
  logDose: async (payload) => data(await apiClient.post("/schedule/log-dose", payload)),
};
export const calendarApi = { get: async (month, year, date) => data(await apiClient.get("/calendar", { params: { month, year, date } })) };
export const historyApi = {
  get: async (params) => data(await apiClient.get("/history", { params })),
  missed: async () => data(await apiClient.get("/history/missed")),
};
export const prescriptionsApi = {
  list: async () => data(await apiClient.get("/prescriptions")),
  get: async (id) => data(await apiClient.get(`/prescriptions/${id}`)),
  upload: async (file) => {
    const form = new FormData();
    form.append("file", file);
    return data(await apiClient.post("/prescriptions", form));
  },
  download: async (id) => apiClient.get(`/prescriptions/${id}/file`, { responseType: "blob" }),
  fileUrl: (id) => `${apiClient.defaults.baseURL}/prescriptions/${id}/file`,
};
export const profileApi = {
  get: async () => data(await apiClient.get("/profile")),
  update: async (payload) => data(await apiClient.put("/profile", payload)),
  changePassword: async (payload) => data(await apiClient.put("/profile/password", payload)),
  deleteAccount: async (payload) => apiClient.delete("/profile", { data: payload }),
};
export const settingsApi = {
  get: async () => data(await apiClient.get("/settings")),
  update: async (payload) => data(await apiClient.put("/settings", payload)),
};
