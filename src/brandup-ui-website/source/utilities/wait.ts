const minWait = (func: (...args) => void, time = 700): () => void => {
    const n = Date.now();

    const ret = (...args) => {
        const n2 = Date.now();
        const w = time - (n2 - n);

        if (w > time * 0.1) {
            window.setTimeout(() => {
                func(...args);
            }, w);
        }
        else {
            func(...args);
        }
    };

    return ret;
};

export default minWait;