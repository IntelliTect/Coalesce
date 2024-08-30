import { userInfo, can } from "@/user-service";

export function useUser() {
  return { userInfo, can };
}
