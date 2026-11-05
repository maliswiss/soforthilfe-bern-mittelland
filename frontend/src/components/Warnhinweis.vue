<script setup>
/// Roter Hinweis direkt unter einem Eingabefeld, mit Warndreieck.
defineProps({
  meldung: { type: String, required: true },
  kennung: { type: String, default: undefined }
});
</script>

<template>
  <p :id="kennung" class="mt-1 flex items-start gap-1.5 text-sm font-medium text-notruf" role="alert">
    <svg
      class="mt-0.5 h-4 w-4 shrink-0"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      <path d="M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0Z" />
      <line x1="12" y1="9" x2="12" y2="13" />
      <line x1="12" y1="17" x2="12.01" y2="17" />
    </svg>
    <span>{{ meldung }}</span>
  </p>
</template>
