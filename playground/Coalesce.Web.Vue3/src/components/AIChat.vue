<template>
  <v-card
    style="
      position: fixed;
      right: 16px;
      bottom: 16px;
      z-index: 1005;
      max-width: 90vw;
      max-height: 90vh;
    "
    :style="minimized ? {} : { width: width + 'px', height: height + 'px' }"
    elevation="10"
    class="d-flex flex-column"
  >
    <div
      class="resize-handle left"
      @mousedown.stop.prevent="startResize($event, 'left')"
    ></div>
    <div
      class="resize-handle top"
      @mousedown.stop.prevent="startResize($event, 'top')"
    ></div>
    <v-card-title
      class="bg-primary text-white d-flex align-center justify-space-between flex-grow-0"
      style="cursor: pointer"
      @click="minimized = !minimized"
    >
      <span class="d-flex align-center">
        <v-icon class="mr-2" size="20">fa-solid fa-robot</v-icon>
        {{ title }}
      </span>
      <div class="d-flex align-center">
        <v-btn
          v-if="!minimized"
          icon
          size="small"
          variant="text"
          @click.stop="resetChat"
          title="Reset chat"
        >
          <i class="fa-solid fa-rotate-left" style="font-size: 18px"></i>
        </v-btn>
        <v-btn
          icon
          size="small"
          variant="text"
          @click.stop="minimized = !minimized"
          title="Minimize"
        >
          <i
            :class="[
              'fa-solid',
              minimized ? 'fa-chevron-up' : 'fa-chevron-down',
            ]"
            style="font-size: 18px"
          ></i>
        </v-btn>
      </div>
    </v-card-title>
    <template v-if="!minimized">
      <v-card-text class="chat-history flex-grow-1" ref="historyRef">
        <div
          v-for="(msg, idx) in messages"
          :key="idx"
          :class="['chat-bubble-row', msg.role]"
        >
          <div :class="['chat-bubble', msg.role, 'text-pre-wrap']">
            {{ msg.content }}
          </div>
        </div>
        <div v-if="endpoint.isLoading" class="chat-bubble-row ai">
          <div
            class="chat-bubble ai text-pre-wrap d-flex align-center"
            style="min-width: 48px; min-height: 32px"
          >
            <v-progress-circular
              indeterminate
              size="20"
              color="primary"
              class="mr-2"
            />
            <span>Thinking…</span>
          </div>
        </div>
        <div
          v-if="!messages.length && !endpoint.isLoading"
          class="text-medium-emphasis text-center py-8"
        >
          No messages yet. Start the conversation!
        </div>
      </v-card-text>
      <v-divider></v-divider>
      <v-card-actions class="d-flex flex-nowrap align-center pa-0 pr-3">
        <v-textarea
          ref="inputRef"
          v-model="input"
          label="Ask a question..."
          auto-grow
          rows="1"
          class="w-100"
          variant="solo"
          @keydown.enter.exact.prevent="send"
          @keydown.up.exact.prevent="fillLastMessage"
          hide-details
        />
        <v-btn
          :disabled="!input.trim()"
          :loading="endpoint.isLoading"
          @click="send"
          color="primary"
          variant="flat"
          prepend-icon="fa fa-paper-plane"
        >
          Send
        </v-btn>
      </v-card-actions>
    </template>
  </v-card>
</template>

<script setup lang="ts">
import { ref, nextTick, watch, useTemplateRef } from "vue";
import { useEdgeResize } from "@/composables/useEdgeResize";
import { AIAgentServiceViewModel } from "@/viewmodels.g";

type CallableEndpointKeys = {
  [K in keyof AIAgentServiceViewModel]: AIAgentServiceViewModel[K] extends {
    invoke: Function;
  }
    ? K
    : never;
}[keyof AIAgentServiceViewModel];

const props = defineProps<{
  title: string;
  endpoint: CallableEndpointKeys;
}>();
const service = new AIAgentServiceViewModel();
const endpoint = service[props.endpoint];

const input = ref("");
const messages = ref<{ role: "user" | "ai"; content: string }[]>([]);
const history = ref<string>("");
const minimized = ref(true);
const inputRef = useTemplateRef("inputRef");
const historyRef = useTemplateRef("historyRef");

const { width, height, startResize } = useEdgeResize();

async function send() {
  const userMsg = input.value.trim();
  if (!userMsg) return;

  pushMessage({ role: "user", content: userMsg });
  input.value = "";
  try {
    const result = await endpoint.invoke(history.value, userMsg);
    history.value = result.history!;
    pushMessage({
      role: "ai",
      content: result.response || "[No response]",
    });
  } catch (e) {
    pushMessage({
      role: "ai",
      content: "[Error: " + endpoint.message + "]",
    });
  }
}

function resetChat() {
  messages.value = [];
  history.value = "";
  input.value = "";
  focusInput();
}

function fillLastMessage() {
  // Find the last user message
  const lastUserMessage = messages.value
    .slice()
    .reverse()
    .find((msg) => msg.role === "user");

  if (lastUserMessage) {
    input.value = lastUserMessage.content;
  }
}

function pushMessage(message: { role: "user" | "ai"; content: string }) {
  messages.value.push(message);
  scrollToBottom();
}

function scrollToBottom() {
  nextTick(() => {
    const el = historyRef.value?.$el || historyRef.value;
    if (el && el.scrollTop !== undefined) {
      el.scrollTop = el.scrollHeight;
    }
  });
}

function focusInput() {
  nextTick(() => {
    inputRef.value?.focus?.();
  });
}

watch(minimized, (val) => {
  if (!val) {
    focusInput();
    scrollToBottom();
  }
});
</script>

<style lang="scss" scoped>
.chat-history {
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  padding: 12px;
  gap: 8px;
}
.chat-bubble-row {
  display: flex;
  &.user {
    justify-content: flex-end;
  }
  &.ai {
    justify-content: flex-start;
  }
  .chat-bubble {
    max-width: 85%;
    padding: 10px 16px;
    border-radius: 18px;
    font-size: 1rem;
    white-space: pre-wrap;
    word-break: break-word;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.07);
    &.user {
      background: #1976d2;
      color: #fff;
      border-bottom-right-radius: 4px;
      border-bottom-left-radius: 18px;
      justify-self: flex-end;
    }
    &.ai {
      background: #f1f1f1;
      color: #222;
      border-bottom-left-radius: 4px;
      border-bottom-right-radius: 18px;
      justify-self: flex-start;
    }
  }
}
.v-theme--dark {
  .chat-bubble.ai {
    background: rgba(var(--v-theme-primary), 0.2);
    color: rgb(var(--v-theme-on-primary));
  }
}
.resize-handle {
  &.left {
    position: absolute;
    left: 0;
    top: 0;
    width: 8px;
    height: 100%;
    cursor: ew-resize;
    z-index: 10;
  }
  &.top {
    position: absolute;
    left: 0;
    top: 0;
    width: 100%;
    height: 8px;
    cursor: ns-resize;
    z-index: 10;
  }
}
</style>
