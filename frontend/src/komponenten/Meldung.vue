<script setup>
// Hinweis- oder Fehlerkasten; die Art steht immer auch als Wort im Titel, nicht nur als Farbe.
defineProps({
  art: { type: String, default: 'fehler' },
  titel: { type: String, required: true },
  text: { type: String, default: '' },
});
</script>

<template>
  <div
    :role="art === 'fehler' ? 'alert' : 'status'"
    class="mt-4 rounded-lg border px-4 py-3"
    :class="art === 'fehler' ? 'border-notfall bg-notfall-hell text-notfall' : 'border-linie bg-hell text-tanne'"
  >
    <p class="font-semibold">{{ titel }}</p>
    <p v-if="text" class="mt-1">{{ text }}</p>
  </div>
</template>
