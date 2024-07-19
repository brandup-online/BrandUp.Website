const minWait = (func: (...args: any[]) => void, minTime: number = 500): () => void => {
    const beginTime = Date.now();

    const ret = (...args: any[]) => {
        const finishTime = Date.now();
        const w = minTime - (finishTime - beginTime);

        if (w > minTime * 0.1)
            window.setTimeout(() => func(...args), w);
        else
            func(...args);
    };

    return ret;
};

const minWaitAsync = (source: Promise<any>, minTime: number = 500): Promise<any> => {
    if (!minTime)
        return source;

    const beginTime = Date.now();

    return source.then(data => {
        const finishTime = Date.now();
        const w = minTime - (finishTime - beginTime);

        if (w > minTime * 0.1)
            return new Promise<any>(resolve => window.setTimeout(() => resolve(data), w));
        else
            return data;
    });
};

export {
    minWait,
    minWaitAsync
}