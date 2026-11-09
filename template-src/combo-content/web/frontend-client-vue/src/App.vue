<script setup lang="ts">
import { onMounted, ref } from 'vue';

interface Greeting {
    text: string;
}

const greetings = ref<Greeting[]>([]);
const text = ref('');

async function loadGreetings(): Promise<void> {
    const response = await fetch('/api/greetings');
    greetings.value = await response.json();
}

async function createGreeting(): Promise<void> {
    if (text.value === '') {
        return;
    }

    await fetch('/api/greetings', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text: text.value }),
    });

    text.value = '';
    await loadGreetings();
}

onMounted(loadGreetings);
</script>

<template>
    <h1>Приветствия</h1>

    <form @submit.prevent="createGreeting">
        <input v-model="text" type="text" />
        <button type="submit">Добавить</button>
    </form>

    <ul>
        <li v-for="greeting in greetings" :key="greeting.text">
            {{ greeting.text }}
        </li>
    </ul>
</template>
