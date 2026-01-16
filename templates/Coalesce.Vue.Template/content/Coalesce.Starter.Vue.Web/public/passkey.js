const browserSupportsPasskeys =
  typeof navigator.credentials !== "undefined" &&
  typeof window.PublicKeyCredential !== "undefined" &&
  typeof window.PublicKeyCredential.parseRequestOptionsFromJSON === "function";

async function fetchWithErrorHandling(url, options = {}) {
  const response = await fetch(url, {
    credentials: "include",
    ...options,
  });
  if (!response.ok) {
    const text = await response.text();
    console.error(text);
    throw new Error(`The server responded with status ${response.status}.`);
  }

  const json = await response.json();

  // Handle Coalesce ItemResult wrapper
  if (json && typeof json === "object" && "wasSuccessful" in json) {
    if (!json.wasSuccessful) {
      throw new Error(json.message || "Request failed");
    }
    // json.object is a string, need to parse it
    return typeof json.object === "string"
      ? JSON.parse(json.object)
      : json.object;
  }

  return json;
}

async function requestCredential(email, mediation, signal) {
  const optionsJson = await fetchWithErrorHandling(
    `/api/PasskeyService/GetRequestOptions`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ username: email }),
      signal,
    },
  );
  const options = PublicKeyCredential.parseRequestOptionsFromJSON(optionsJson);
  return await navigator.credentials.get({
    publicKey: options,
    mediation,
    signal,
  });
}

customElements.define(
  "passkey-submit",
  class extends HTMLElement {
    static formAssociated = true;

    connectedCallback() {
      this.internals = this.attachInternals();
      this.attrs = {
        operation: this.getAttribute("operation"),
        name: this.getAttribute("name"),
        emailName: this.getAttribute("email-name"),
      };

      this.internals.form.addEventListener("submit", (event) => {
        if (event.submitter?.name === "__passkeySubmit") {
          event.preventDefault();
          this.obtainAndSubmitCredential();
        }
      });

      this.tryAutofillPasskey();
    }

    disconnectedCallback() {
      this.abortController?.abort();
    }

    async obtainCredential(useConditionalMediation, signal) {
      if (!browserSupportsPasskeys) {
        throw new Error(
          "Some passkey features are missing. Please update your browser.",
        );
      }

      const email = new FormData(this.internals.form).get(this.attrs.emailName);
      const mediation = useConditionalMediation ? "conditional" : undefined;
      return await requestCredential(email, mediation, signal);
    }

    async obtainAndSubmitCredential(useConditionalMediation = false) {
      this.abortController?.abort();
      this.abortController = new AbortController();
      const signal = this.abortController.signal;
      const formData = new FormData();

      // Clear any existing error message
      this.clearError();

      try {
        const credential = await this.obtainCredential(
          useConditionalMediation,
          signal,
        );
        const credentialJson = JSON.stringify(credential);
        formData.append(`CredentialJson`, credentialJson);
        this.internals.setFormValue(formData);
        this.internals.form.submit();
      } catch (error) {
        if (error.name === "AbortError") {
          // The user explicitly canceled the operation - return without error.
          return;
        }
        console.error(error);
        if (useConditionalMediation) {
          // An error occurred during conditional mediation, which is not user-initiated.
          // We log the error in the console but do not relay it to the user.
          return;
        }
        const errorMessage =
          error.name === "NotAllowedError"
            ? "No passkey was provided by the authenticator."
            : error.message;
        this.showError(errorMessage);
      }
    }

    showError(message) {
      // Find or create error container
      let errorDiv = this.internals.form.querySelector(".passkey-error");
      if (!errorDiv) {
        errorDiv = document.createElement("div");
        errorDiv.className = "passkey-error text-danger";
        errorDiv.style.marginTop = "0.5rem";
        // Insert after the passkey-submit element
        this.parentElement.insertBefore(errorDiv, this.nextSibling);
      }
      errorDiv.textContent = message;
    }

    clearError() {
      const errorDiv = this.internals.form.querySelector(".passkey-error");
      if (errorDiv) {
        errorDiv.remove();
      }
    }

    async tryAutofillPasskey() {
      if (
        browserSupportsPasskeys &&
        (await PublicKeyCredential.isConditionalMediationAvailable?.())
      ) {
        await this.obtainAndSubmitCredential(
          /* useConditionalMediation */ true,
        );
      }
    }
  },
);
