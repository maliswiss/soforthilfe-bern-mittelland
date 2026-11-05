<script setup>
/// Hinweis oder Fehlermeldung als Kasten mit farbigem Rand.
defineProps({
  art: { type: String, default: 'hinweis' },
  meldung: { type: String, required: true }
});
</script>

<template>
  <p
    v-if="meldung"
    class="border-l-4 px-4 py-3 text-sm"
    :class="art === 'fehler'
      ? 'border-notruf bg-white text-notruf'
      : 'border-tanne bg-tanne-blass text-tinte'"
    :role="art === 'fehler' ? 'alert' : 'status'"
  >
    {{ meldung }}
  </p>
</template>
