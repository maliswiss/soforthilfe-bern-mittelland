<script setup>
defineOptions({ inheritAttrs: false });
// Eingabefeld mit fester Beschriftung und optionalem Hinweis darunter.
defineProps({
  id: { type: String, required: true },
  beschriftung: { type: String, required: true },
  hinweis: { type: String, default: '' },
});
const wert = defineModel({ type: String, default: '' });
</script>

<template>
  <div class="mt-5">
    <label :for="id" class="block font-semibold text-tanne">{{ beschriftung }}</label>
    <input
      :id="id"
      v-model="wert"
      v-bind="$attrs"
      :aria-describedby="hinweis ? `${id}-hinweis` : undefined"
      class="mt-1 w-full rounded-lg border border-rand bg-white px-3 py-3 text-lg"
    />
    <p v-if="hinweis" :id="`${id}-hinweis`" class="mt-1 text-sm text-gedaempft">{{ hinweis }}</p>
  </div>
</template>
