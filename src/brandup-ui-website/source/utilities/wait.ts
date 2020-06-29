
const minWait = (func: (...args) => void, time: number = 700): () => void => {
    let n = Date.now();

    var ret = (...args) => {
        let n2 = Date.now();
        let w = time - (n2 - n);

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