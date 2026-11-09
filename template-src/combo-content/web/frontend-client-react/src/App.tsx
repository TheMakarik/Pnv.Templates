import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';

interface Greeting {
    text: string;
}

export default function App() {
    const [greetings, setGreetings] = useState<Greeting[]>([]);
    const [text, setText] = useState('');

    const loadGreetings = async (): Promise<void> => {
        const response = await fetch('/api/greetings');
        setGreetings(await response.json());
    };

    const createGreeting = async (): Promise<void> => {
        if (text === '') {
            return;
        }

        await fetch('/api/greetings', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ text }),
        });

        setText('');
        await loadGreetings();
    };

    useEffect(() => {
        void loadGreetings();
    }, []);

    return (
        <>
            <h1>Приветствия</h1>
            <form
                onSubmit={(event: FormEvent) => {
                    event.preventDefault();
                    void createGreeting();
                }}
            >
                <input value={text} onChange={(event) => setText(event.target.value)} />
                <button type="submit">Добавить</button>
            </form>
            <ul>
                {greetings.map((greeting) => (
                    <li key={greeting.text}>{greeting.text}</li>
                ))}
            </ul>
        </>
    );
}
