declare module 'edge-js' {
    function func<TInput, TOutput>(params: string | Function | Params | Source): Func<TInput, TOutput>
    interface Params {
        assemblyFile: string
        typeName?: string
        methodName?: string
    }

    interface Source {
        source: string | Function
        references: string[]
    }

    interface Func<TInput, TOutput> {
        (payload: TInput, callback: (error: Error, result: TOutput) => void): void;
        (payload: TInput, sync: true): TOutput;
    }
}