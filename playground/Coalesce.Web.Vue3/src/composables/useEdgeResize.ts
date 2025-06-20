import { ref, onBeforeUnmount } from "vue";

export function useEdgeResize({
  initialWidth = 400,
  initialHeight = 500,
  minWidth = 300,
  minHeight = 200,
} = {}) {
  const width = ref(initialWidth);
  const height = ref(initialHeight);

  let resizing = false;
  let resizeDir: "left" | "top" | null = null;
  let startX = 0;
  let startY = 0;
  let startWidth = 0;
  let startHeight = 0;

  function startResize(e: MouseEvent, dir: "left" | "top") {
    resizing = true;
    resizeDir = dir;
    startX = e.clientX;
    startY = e.clientY;
    startWidth = width.value;
    startHeight = height.value;
    document.addEventListener("mousemove", onResize);
    document.addEventListener("mouseup", stopResize);
  }

  function onResize(e: MouseEvent) {
    if (!resizing || !resizeDir) return;
    if (resizeDir === "left") {
      const delta = startX - e.clientX;
      width.value = Math.max(minWidth, startWidth + delta);
    } else if (resizeDir === "top") {
      const delta = startY - e.clientY;
      height.value = Math.max(minHeight, startHeight + delta);
    }
  }

  function stopResize() {
    resizing = false;
    resizeDir = null;
    document.removeEventListener("mousemove", onResize);
    document.removeEventListener("mouseup", stopResize);
  }

  onBeforeUnmount(() => {
    stopResize();
  });

  return {
    width,
    height,
    startResize,
  };
}
