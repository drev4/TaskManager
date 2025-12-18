<template>
  <div class="space-y-6">
    <div class="bg-white shadow rounded-lg p-6">
      <h1 class="text-2xl font-bold text-gray-900">Project Details</h1>
      <p class="mt-2 text-gray-600">Project ID: {{ id }}</p>
      <p class="mt-4 text-gray-500">This page is under construction.</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { defineProps } from 'vue'

defineProps<{
  id: string
}>()
</script>
