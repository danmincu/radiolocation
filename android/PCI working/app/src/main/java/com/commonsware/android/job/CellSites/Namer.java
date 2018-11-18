package com.commonsware.android.job.CellSites;

/* renamed from: j */
public final class Namer {
    /* renamed from: a */
    public static String getName(Class<?> cls) {
        String simpleName = cls.getSimpleName();
        if (simpleName.length() > 14) {
            simpleName = simpleName.substring(simpleName.length() - 14, simpleName.length());
        }
        return "Stumbler_" + simpleName;
    }
}
