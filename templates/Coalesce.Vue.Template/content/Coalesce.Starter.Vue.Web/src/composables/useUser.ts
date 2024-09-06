import {
  userInfo,
  //#if Identity
  can,
  //#endif
} from "@/user-service";

export function useUser() {
  return {
    userInfo,
    //#if Identity
    can,
    //#endif
  };
}
