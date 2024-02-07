#include <iostream>
#include <vector>
#include <memory>
#include <cstdlib>

#if defined(_WIN32)
#include <malloc.h>
#elif defined(__linux__)
#include <cstdlib>
#endif

template<typename T, size_t Alignment>
class AlignedAllocator {
public:
    using value_type = T;
    using pointer = T*;
    using const_pointer = const T*;
    using size_type = std::size_t;
    using difference_type = std::ptrdiff_t;

    template<typename U>
    struct rebind {
        using other = AlignedAllocator<U, Alignment>;
    };

    AlignedAllocator() noexcept = default;

    template<typename U>
    AlignedAllocator(const AlignedAllocator<U, Alignment>&) noexcept {}

    pointer allocate(size_type n) {
        void* p;
#if defined(_WIN32)
        p = _aligned_malloc(n * sizeof(T), Alignment);
        if (!p) throw std::bad_alloc();
#elif defined(__linux__)
        if (posix_memalign(&p, Alignment, n * sizeof(T)) != 0) {
            throw std::bad_alloc();
        }
#endif
        return static_cast<pointer>(p);
    }

    void deallocate(pointer p, size_type) noexcept {
#if defined(_WIN32)
        _aligned_free(p);
#elif defined(__linux__)
        std::free(p);
#endif
    }
};

int main() {
    // Define a vector of doubles with custom allocator with 64-byte alignment
    std::vector<double, AlignedAllocator<double, 64>> vec;

    // Push some elements
    vec.push_back(3.14);
    vec.push_back(2.718);

    // Access elements
    for (const auto& elem : vec) {
        std::cout << elem << std::endl;
    }

    return 0;
}

/*
#include <boost/align/aligned_allocator.hpp>
#include <vector>
int main()
{
  std::vector<double, boost::alignment::aligned_allocator<double, 64>> vec;
}
*/
